using RAGChatbot.API.Models;
using System.Text.Json;

namespace RAGChatbot.API.Services;

public class FAISSVectorStore : IVectorStore
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FAISSVectorStore> _logger;
    private readonly string _storagePath;
    private readonly int _dimension;
    private List<DocumentChunk> _documents = new();
    
    public FAISSVectorStore(IConfiguration configuration, ILogger<FAISSVectorStore> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _storagePath = _configuration["VectorStore:StoragePath"] ?? "./vectorstore";
        _dimension = int.Parse(_configuration["VectorStore:Dimension"] ?? "1536");
    }

    public async Task InitializeAsync()
    {
        try
        {
            Directory.CreateDirectory(_storagePath);
            
            // Load existing documents if available
            var metadataPath = Path.Combine(_storagePath, "metadata.json");
            if (File.Exists(metadataPath))
            {
                var json = await File.ReadAllTextAsync(metadataPath);
                _documents = JsonSerializer.Deserialize<List<DocumentChunk>>(json) ?? new();
                _logger.LogInformation("Loaded {Count} documents from storage", _documents.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing vector store");
            throw;
        }
    }

    public async Task AddDocumentsAsync(List<DocumentChunk> chunks)
    {
        try
        {
            _documents.AddRange(chunks);
            
            // Save metadata
            var metadataPath = Path.Combine(_storagePath, "metadata.json");
            var json = JsonSerializer.Serialize(_documents, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(metadataPath, json);
            
            _logger.LogInformation("Added {Count} chunks to vector store", chunks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding documents to vector store");
            throw;
        }
    }

    public async Task<List<DocumentChunk>> SearchAsync(float[] queryEmbedding, int topK = 5)
    {
        try
        {
            if (_documents.Count == 0)
                return new List<DocumentChunk>();
            
            // Calculate cosine similarity for each document
            var scoredDocs = _documents
                .Select(doc => new
                {
                    Document = doc,
                    Score = CosineSimilarity(queryEmbedding, doc.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Document)
                .ToList();
            
            return scoredDocs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vector store");
            throw;
        }
    }

    public async Task<bool> ClearAsync()
    {
        try
        {
            _documents.Clear();
            var metadataPath = Path.Combine(_storagePath, "metadata.json");
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
            _logger.LogInformation("Cleared all documents from vector store");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing vector store");
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentName)
    {
        try
        {
            var initialCount = _documents.Count;
            _documents.RemoveAll(d => d.DocumentName.Equals(documentName, StringComparison.OrdinalIgnoreCase));
            var removedCount = initialCount - _documents.Count;
            
            if (removedCount > 0)
            {
                // Save updated metadata
                var metadataPath = Path.Combine(_storagePath, "metadata.json");
                var json = JsonSerializer.Serialize(_documents, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(metadataPath, json);
                
                _logger.LogInformation("Deleted {Count} chunks from document: {DocumentName}", removedCount, documentName);
                return true;
            }
            
            _logger.LogWarning("No documents found with name: {DocumentName}", documentName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentName}", documentName);
            return false;
        }
    }

    public async Task<List<string>> GetAllDocumentNamesAsync()
    {
        try
        {
            var documentNames = _documents
                .Select(d => d.DocumentName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            
            return documentNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document names");
            return new List<string>();
        }
    }

    public async Task<List<DocumentChunk>> GetAllChunksAsync()
    {
        try
        {
            return _documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all chunks");
            return new List<DocumentChunk>();
        }
    }

    private float CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must have the same length");
        
        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;
        
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }
        
        magnitudeA = (float)Math.Sqrt(magnitudeA);
        magnitudeB = (float)Math.Sqrt(magnitudeB);
        
        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;
        
        return dotProduct / (magnitudeA * magnitudeB);
    }
}
