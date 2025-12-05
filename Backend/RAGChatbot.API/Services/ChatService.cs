using RAGChatbot.API.Models;

namespace RAGChatbot.API.Services;

public class ChatService : IChatService
{
    private readonly IDocumentProcessor _documentProcessor;
    private readonly ITextSplitter _textSplitter;
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILLMService _llmService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IDocumentProcessor documentProcessor,
        ITextSplitter textSplitter,
        IVectorStore vectorStore,
        IEmbeddingService embeddingService,
        ILLMService llmService,
        ILogger<ChatService> logger)
    {
        _documentProcessor = documentProcessor;
        _textSplitter = textSplitter;
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _llmService = llmService;
        _logger = logger;
        
        // Initialize vector store
        try
        {
            _vectorStore.InitializeAsync().Wait();
            _logger.LogInformation("Vector store initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize vector store - it will be initialized on first use");
        }
    }

    public async Task<ChatResponse> ProcessChatAsync(ChatRequest request)
    {
        try
        {
            // Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Message);
            
            // Search for relevant chunks
            var relevantChunks = await _vectorStore.SearchAsync(queryEmbedding, request.TopK);
            
            if (relevantChunks.Count == 0)
            {
                return new ChatResponse
                {
                    SessionId = request.SessionId,
                    Message = request.Message,
                    Response = "I don't have any relevant information to answer that question. Please upload some documents first.",
                    Sources = new List<SourceReference>()
                };
            }
            
            // Extract context
            var context = relevantChunks.Select(c => c.Content).ToList();
            
            // Generate response using LLM
            var response = await _llmService.GenerateResponseAsync(request.Message, context);
            
            // Build source references - extract specific answer if possible
            var sources = relevantChunks.Select((chunk, index) => new SourceReference
            {
                DocumentName = chunk.DocumentName,
                ChunkIndex = chunk.ChunkIndex,
                Text = ExtractRelevantPortion(request.Message, chunk.Content),
                Score = 0.0
            }).ToList();
            
            return new ChatResponse
            {
                SessionId = request.SessionId,
                Message = request.Message,
                Response = response,
                Sources = sources
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            throw;
        }
    }

    public async Task<DocumentUploadResponse> ProcessDocumentAsync(IFormFile file, string topic)
    {
        try
        {
            _logger.LogInformation("Processing document: {FileName} ({Length} bytes)", file.FileName, file.Length);
            
            string content;
            var extension = Path.GetExtension(file.FileName).ToLower();
            
            // Copy file to memory stream first to avoid stream positioning issues
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            content = extension switch
            {
                ".pdf" => await _documentProcessor.ProcessPdfAsync(memoryStream, file.FileName),
                ".csv" => await _documentProcessor.ProcessCsvAsync(memoryStream),
                ".xlsx" or ".xls" => await _documentProcessor.ProcessExcelAsync(memoryStream),
                ".txt" => await _documentProcessor.ProcessTextFileAsync(memoryStream),
                ".png" or ".jpg" or ".jpeg" => await _documentProcessor.ProcessImageAsync(memoryStream),
                _ => throw new NotSupportedException($"File type {extension} is not supported")
            };
            
            // Split into chunks
            var chunks = _textSplitter.SplitText(content);
            
            // Generate embeddings for all chunks
            var embeddings = await _embeddingService.GenerateBatchEmbeddingsAsync(chunks);
            
            // Create document chunks
            var documentChunks = chunks.Select((chunk, index) => new DocumentChunk
            {
                DocumentName = file.FileName,
                Topic = topic,
                Content = chunk,
                ChunkIndex = index,
                Embedding = embeddings[index],
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = file.FileName,
                    ["topic"] = topic,
                    ["uploadDate"] = DateTime.UtcNow.ToString("O")
                }
            }).ToList();
            
            // Store in vector database
            await _vectorStore.AddDocumentsAsync(documentChunks);
            
            return new DocumentUploadResponse
            {
                Success = true,
                Message = $"Successfully processed {file.FileName}",
                TotalChunks = chunks.Count,
                ProcessedFiles = new List<string> { file.FileName }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {FileName}", file.FileName);
            return new DocumentUploadResponse
            {
                Success = false,
                Message = $"Error processing document: {ex.Message}",
                TotalChunks = 0,
                ProcessedFiles = new List<string>()
            };
        }
    }

    public async Task<DocumentUploadResponse> ProcessVideoTranscriptAsync(string videoUrl, string topic)
    {
        try
        {
            // Get video transcript
            var transcript = await _documentProcessor.GetVideoTranscriptAsync(videoUrl);
            
            // Split into chunks
            var chunks = _textSplitter.SplitText(transcript);
            
            // Generate embeddings for all chunks
            var embeddings = await _embeddingService.GenerateBatchEmbeddingsAsync(chunks);
            
            // Create document chunks
            var documentChunks = chunks.Select((chunk, index) => new DocumentChunk
            {
                DocumentName = $"Video: {videoUrl}",
                Topic = topic,
                Content = chunk,
                ChunkIndex = index,
                Embedding = embeddings[index],
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = videoUrl,
                    ["topic"] = topic,
                    ["type"] = "video-transcript",
                    ["uploadDate"] = DateTime.UtcNow.ToString("O")
                }
            }).ToList();
            
            // Store in vector database
            await _vectorStore.AddDocumentsAsync(documentChunks);
            
            return new DocumentUploadResponse
            {
                Success = true,
                Message = $"Successfully processed video transcript",
                TotalChunks = chunks.Count,
                ProcessedFiles = new List<string> { videoUrl }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video transcript: {VideoUrl}", videoUrl);
            return new DocumentUploadResponse
            {
                Success = false,
                Message = $"Error processing video: {ex.Message}",
                TotalChunks = 0,
                ProcessedFiles = new List<string>()
            };
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentName)
    {
        try
        {
            return await _vectorStore.DeleteDocumentAsync(documentName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentName}", documentName);
            return false;
        }
    }

    public async Task<bool> ClearAllDocumentsAsync()
    {
        try
        {
            return await _vectorStore.ClearAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all documents");
            return false;
        }
    }

    public async Task<List<string>> GetAllDocumentsAsync()
    {
        try
        {
            return await _vectorStore.GetAllDocumentNamesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all documents");
            return new List<string>();
        }
    }

    private string ExtractRelevantPortion(string question, string content)
    {
        var questionLower = question.ToLower();
        
        // Check if asking about a specific step number
        var stepMatch = System.Text.RegularExpressions.Regex.Match(questionLower, @"step\s*(\d+)");
        if (stepMatch.Success)
        {
            var stepNumber = stepMatch.Groups[1].Value;
            
            // Extract only that specific step from the content
            var patterns = new[]
            {
                $@"#\s*{stepNumber}\s+([^\n#]+(?:\n(?!#\s*\d)[^\n]*)*)",  // # 8 Build UI...
                $@"Step\s*{stepNumber}[:\s]+([^\n]+(?:\n(?!Step\s*\d)[^\n]+)*)"  // Step 8: ...
            };
            
            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    content, 
                    pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline
                );
                
                if (match.Success)
                {
                    var extracted = match.Groups[1].Value.Trim();
                    _logger.LogInformation($"Extracted step {stepNumber} from source");
                    return $"# {stepNumber} {extracted}";
                }
            }
        }
        
        // If no specific step requested or couldn't extract, return limited content
        return content.Length > 500 ? content.Substring(0, 500) + "..." : content;
    }

    public async Task<Quiz> GenerateQuizAsync(string topic, int questionCount)
    {
        try
        {
            _logger.LogInformation("Generating quiz for topic: {Topic}, Questions: {Count}", topic, questionCount);

            // Get all documents or filter by topic
            var allChunks = await _vectorStore.GetAllChunksAsync();
            var relevantChunks = allChunks
                .Where(c => string.IsNullOrWhiteSpace(topic) || c.DocumentName.Contains(topic, StringComparison.OrdinalIgnoreCase))
                .Take(20) // Limit to prevent too much context
                .ToList();

            if (relevantChunks.Count == 0)
            {
                throw new Exception($"No documents found for topic: {topic}");
            }

            // Combine content from relevant chunks
            var contextParts = relevantChunks.Select(c => c.Content).ToList();
            var context = string.Join("\n\n", contextParts);

            // Generate quiz using LLM
            var quiz = await _llmService.GenerateQuizAsync(context, topic, questionCount);
            
            _logger.LogInformation("Successfully generated {Count} quiz questions", quiz.Questions.Count);
            return quiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            throw;
        }
    }
}
