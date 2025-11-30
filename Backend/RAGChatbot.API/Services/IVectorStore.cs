using RAGChatbot.API.Models;

namespace RAGChatbot.API.Services;

public interface IVectorStore
{
    Task InitializeAsync();
    Task AddDocumentsAsync(List<DocumentChunk> chunks);
    Task<List<DocumentChunk>> SearchAsync(float[] queryEmbedding, int topK = 5);
    Task<bool> ClearAsync();
    Task<bool> DeleteDocumentAsync(string documentName);
    Task<List<string>> GetAllDocumentNamesAsync();
}
