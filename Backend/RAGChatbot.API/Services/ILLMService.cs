namespace RAGChatbot.API.Services;

public interface ILLMService
{
    Task<string> GenerateResponseAsync(string prompt, List<string> context);
}
