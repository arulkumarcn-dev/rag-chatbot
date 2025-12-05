namespace RAGChatbot.API.Services;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts);
}
