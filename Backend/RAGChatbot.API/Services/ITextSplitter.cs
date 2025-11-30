namespace RAGChatbot.API.Services;

public interface ITextSplitter
{
    List<string> SplitText(string text, int chunkSize = 1000, int overlap = 200);
}
