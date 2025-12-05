using RAGChatbot.API.Models;

namespace RAGChatbot.API.Services;

public interface ILLMService
{
    Task<string> GenerateResponseAsync(string prompt, List<string> context);
    Task<Quiz> GenerateQuizAsync(string context, string topic, int questionCount);
}
