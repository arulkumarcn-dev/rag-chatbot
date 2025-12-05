using System.Text;

namespace RAGChatbot.API.Services;

public class TextSplitter : ITextSplitter
{
    public List<string> SplitText(string text, int chunkSize = 1000, int overlap = 200)
    {
        var chunks = new List<string>();
        
        if (string.IsNullOrWhiteSpace(text))
            return chunks;
        
        // Split by paragraphs first
        var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        var currentChunk = new StringBuilder();
        
        foreach (var paragraph in paragraphs)
        {
            // If adding this paragraph exceeds chunk size
            if (currentChunk.Length + paragraph.Length > chunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                
                // Keep overlap from the end of the previous chunk
                var overlapText = GetOverlapText(currentChunk.ToString(), overlap);
                currentChunk.Clear();
                currentChunk.Append(overlapText);
            }
            
            currentChunk.AppendLine(paragraph);
            currentChunk.AppendLine();
        }
        
        // Add the last chunk
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }
        
        return chunks;
    }
    
    private string GetOverlapText(string text, int overlapSize)
    {
        if (text.Length <= overlapSize)
            return text;
        
        // Try to find a sentence or word boundary
        var overlapText = text.Substring(text.Length - overlapSize);
        var lastPeriod = overlapText.LastIndexOf('.');
        
        if (lastPeriod > 0)
        {
            overlapText = overlapText.Substring(lastPeriod + 1).Trim();
        }
        
        return overlapText;
    }
}
