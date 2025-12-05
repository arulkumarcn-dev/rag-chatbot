// Mock Embedding Service for Testing (No OpenAI Required)
using System.Security.Cryptography;

namespace RAGChatbot.API.Services;

public class MockEmbeddingService : IEmbeddingService
{
    private readonly ILogger<MockEmbeddingService> _logger;
    private readonly int _dimension;

    public MockEmbeddingService(ILogger<MockEmbeddingService> logger)
    {
        _logger = logger;
        _dimension = 1536; // Standard OpenAI embedding dimension
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        _logger.LogInformation("Generating mock embedding for text (length: {Length})", text.Length);
        return GenerateMockEmbedding(text);
    }

    public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts)
    {
        _logger.LogInformation("Generating mock embeddings for {Count} texts", texts.Count);
        return texts.Select(t => GenerateMockEmbedding(t)).ToList();
    }

    private float[] GenerateMockEmbedding(string text)
    {
        // Generate deterministic embeddings based on text content
        var embedding = new float[_dimension];
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text));
        
        var random = new Random(BitConverter.ToInt32(hash, 0));
        for (int i = 0; i < _dimension; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1); // Range: -1 to 1
        }
        
        // Normalize
        var magnitude = Math.Sqrt(embedding.Sum(x => x * x));
        for (int i = 0; i < _dimension; i++)
        {
            embedding[i] /= (float)magnitude;
        }
        
        return embedding;
    }
}
