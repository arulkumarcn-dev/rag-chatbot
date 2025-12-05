namespace RAGChatbot.API.Services;

public interface IDocumentProcessor
{
    Task<string> ProcessPdfAsync(Stream fileStream, string fileName);
    Task<string> ProcessCsvAsync(Stream fileStream);
    Task<string> ProcessExcelAsync(Stream fileStream);
    Task<string> ProcessImageAsync(Stream fileStream);
    Task<string> GetVideoTranscriptAsync(string videoUrl);
    Task<string> ProcessTextFileAsync(Stream fileStream);
}

