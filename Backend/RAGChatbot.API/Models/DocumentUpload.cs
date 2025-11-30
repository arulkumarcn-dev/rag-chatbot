namespace RAGChatbot.API.Models;

public class DocumentUploadRequest
{
    public string Topic { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
}

public class DocumentUploadResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalChunks { get; set; }
    public List<string> ProcessedFiles { get; set; } = new();
}

public class DocumentChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
