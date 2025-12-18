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
            
            // Search for MORE relevant chunks initially to ensure we get the right content
            var relevantChunks = await _vectorStore.SearchAsync(queryEmbedding, 25);
            
            if (relevantChunks.Count == 0)
            {
                // Try to generate a general response without context
                var generalResponse = await _llmService.GenerateResponseAsync(request.Message, new List<string>());
                
                return new ChatResponse
                {
                    SessionId = request.SessionId,
                    Message = request.Message,
                    Response = generalResponse,
                    Sources = new List<SourceReference>()
                };
            }
            
            // FILTER OUT IMAGE-ONLY CHUNKS - prioritize text content
            var textChunks = relevantChunks
                .Where(c => !IsImageOnlyChunk(c.Content))
                .Where(c => c.Content.Length > 50) // Minimum meaningful content length
                .ToList();
            
            // If we filtered out too many, use original but still prefer text
            if (textChunks.Count < 5)
            {
                _logger.LogWarning("Few text chunks found, using more chunks");
                textChunks = relevantChunks.Where(c => c.Content.Length > 30).Take(15).ToList();
            }
            
            // Extract FULL context from chunks - take more for better answers
            var context = textChunks.Take(15).Select(c => c.Content).ToList();
            
            // For numbered section queries or page-specific queries, get even more context
            if (System.Text.RegularExpressions.Regex.IsMatch(request.Message, @"\d+\.\d+|page\s*\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                _logger.LogInformation("Detected section/page query, fetching maximum context");
                // Get top 20 chunks for section/page queries to ensure complete content
                var sectionChunks = await _vectorStore.SearchAsync(queryEmbedding, 20);
                var textSectionChunks = sectionChunks.Where(c => !IsImageOnlyChunk(c.Content) && c.Content.Length > 30).ToList();
                context = textSectionChunks.Take(15).Select(c => c.Content).ToList();
                textChunks = textSectionChunks;
            }
            
            _logger.LogInformation("Using {Count} text chunks for answer generation", context.Count);
            
            // Generate response using LLM with full context
            var response = await _llmService.GenerateResponseAsync(request.Message, context);
            
            // Build source references - show actual relevant content, text only
            var sources = textChunks.Take(5).Select((chunk, index) => new SourceReference
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
        
        // Check for section numbers like "1.1", "2.3", etc.
        var sectionMatch = System.Text.RegularExpressions.Regex.Match(questionLower, @"(\d+\.\d+)");
        if (sectionMatch.Success)
        {
            var sectionNumber = sectionMatch.Groups[1].Value;
            
            // Try to find this section in the content with various patterns
            var patterns = new[]
            {
                $@"{sectionNumber}[:\s-]+([^\n]+(?:\n(?!\d+\.)[^\n]+){{0,5}})",  // 1.1: content (up to 5 more lines)
                $@"Section\s*{sectionMatch.Groups[1].Value}[:\s]+([^\n]+(?:\n(?!Section)[^\n]+){{0,5}})"  // Section 1.1: content
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
                    _logger.LogInformation($"Extracted section {sectionNumber}: {extracted.Substring(0, Math.Min(50, extracted.Length))}...");
                    return $"Section {sectionNumber}: {extracted}";
                }
            }
            
            // If pattern didn't work, find the section number and extract context
            var index = content.IndexOf(sectionNumber);
            if (index >= 0)
            {
                var start = Math.Max(0, index - 50);
                var length = Math.Min(800, content.Length - start);
                var contextSnippet = content.Substring(start, length).Trim();
                return contextSnippet;
            }
        }
        
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
                    return $"Step {stepNumber}: {extracted}";
                }
            }
        }
        
        // For general queries, try to extract the most relevant sentence
        var queryWords = questionLower.Split(new[] { ' ', '?', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var sentences = content.Split(new[] { '.', '!', '?', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Find sentence with most query word matches
        var bestMatch = sentences
            .Select(s => new { 
                Sentence = s.Trim(), 
                Score = queryWords.Count(w => s.ToLower().Contains(w)) 
            })
            .Where(x => x.Score > 0 && x.Sentence.Length > 20)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();
        
        if (bestMatch != null)
        {
            var sentence = bestMatch.Sentence;
            if (sentence.Length > 500)
            {
                return sentence.Substring(0, 500) + "...";
            }
            return sentence;
        }
        
        // If no specific match, return beginning of content
        return content.Length > 600 ? content.Substring(0, 600) + "..." : content;
    }

    private bool IsImageOnlyChunk(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length < 20)
            return true;
        
        // Check for common image-only patterns
        var imagePatterns = new[]
        {
            "[Image",
            "Figure ",
            "Fig.",
            "Diagram",
            "Chart",
            "Graph",
            "Photo",
            "Picture",
            "Illustration"
        };
        
        var lowerContent = content.ToLower().Trim();
        
        // If content is very short and contains image keywords, it's likely image-only
        if (content.Length < 100)
        {
            foreach (var pattern in imagePatterns)
            {
                if (lowerContent.StartsWith(pattern.ToLower()) || lowerContent.Contains(pattern.ToLower() + ":"))
                    return true;
            }
        }
        
        // Check if content is mostly numbers/special chars (likely image metadata)
        var alphaCount = content.Count(char.IsLetter);
        var totalCount = content.Length;
        if (totalCount > 0 && (double)alphaCount / totalCount < 0.3)
            return true;
        
        return false;
    }

    public async Task<Quiz> GenerateQuizAsync(string topic, int questionCount)
    {
        try
        {
            _logger.LogInformation("Generating quiz for topic: {Topic}, Questions: {Count}", topic, questionCount);

            // Get all documents or filter by topic
            var allChunks = await _vectorStore.GetAllChunksAsync();
            var filteredChunks = allChunks
                .Where(c => string.IsNullOrWhiteSpace(topic) || c.DocumentName.Contains(topic, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filteredChunks.Count == 0)
            {
                throw new Exception($"No documents found for topic: {topic}");
            }

            // Calculate chunks needed - more questions = more chunks for better coverage
            // For large quizzes, sample from different parts of the document
            int chunksNeeded = questionCount switch
            {
                <= 10 => Math.Min(20, filteredChunks.Count),
                <= 20 => Math.Min(40, filteredChunks.Count),
                <= 50 => Math.Min(100, filteredChunks.Count),
                <= 100 => Math.Min(200, filteredChunks.Count),
                <= 150 => Math.Min(300, filteredChunks.Count),
                _ => Math.Min(400, filteredChunks.Count) // For 200 questions
            };

            // Sample chunks evenly across the document for maximum coverage
            List<DocumentChunk> relevantChunks;
            if (filteredChunks.Count <= chunksNeeded)
            {
                relevantChunks = filteredChunks;
            }
            else
            {
                // Sample evenly across the document
                var step = (double)filteredChunks.Count / chunksNeeded;
                relevantChunks = new List<DocumentChunk>();
                for (int i = 0; i < chunksNeeded; i++)
                {
                    var index = (int)(i * step);
                    relevantChunks.Add(filteredChunks[index]);
                }
            }

            _logger.LogInformation("Selected {ChunkCount} chunks from {TotalChunks} total for {QuestionCount} questions",
                relevantChunks.Count, filteredChunks.Count, questionCount);

            // Combine content from relevant chunks
            var contextParts = relevantChunks.Select(c => c.Content).ToList();
            var context = string.Join("\n\n", contextParts);

            // Generate quiz using LLM (it will handle batching internally)
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
