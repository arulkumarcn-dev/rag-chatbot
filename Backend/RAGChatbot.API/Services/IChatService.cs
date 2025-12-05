using RAGChatbot.API.Models;

namespace RAGChatbot.API.Services;

public interface IChatService
{
    Task<ChatResponse> ProcessChatAsync(ChatRequest request);
    Task<DocumentUploadResponse> ProcessDocumentAsync(IFormFile file, string topic);
    Task<DocumentUploadResponse> ProcessVideoTranscriptAsync(string videoUrl, string topic);
    Task<bool> DeleteDocumentAsync(string documentName);
    Task<bool> ClearAllDocumentsAsync();
    Task<List<string>> GetAllDocumentsAsync();
    Task<Quiz> GenerateQuizAsync(string topic, int questionCount);
}
