using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RAGChatbot.API.Services;

public class RobustLLMService : ILLMService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RobustLLMService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly bool _useMockMode;

    public RobustLLMService(IConfiguration configuration, ILogger<RobustLLMService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
        
        _apiKey = _configuration["OpenAI:ApiKey"] ?? "";
        _model = _configuration["OpenAI:ChatModel"] ?? "gpt-4";
        
        _useMockMode = string.IsNullOrEmpty(_apiKey) || 
                       _apiKey == "your-openai-api-key-here" ||
                       !_apiKey.StartsWith("sk-");
        
        if (_useMockMode)
        {
            _logger.LogWarning("⚠️ Using MOCK responses (OpenAI API key not configured or invalid)");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _logger.LogInformation("✓ Using OpenAI chat completions");
        }
    }

    public async Task<string> GenerateResponseAsync(string prompt, List<string> context)
    {
        if (_useMockMode)
        {
            return GenerateMockResponse(prompt, context);
        }

        try
        {
            var contextText = string.Join("\n\n", context.Select((c, i) => $"[Context {i + 1}]\n{c}"));
            
            var systemMessage = @"You are a helpful AI assistant that answers questions based on the provided context. 
Always cite the context number when using information from it (e.g., 'According to Context 1...').
If the context doesn't contain relevant information to answer the question, say so clearly.
Be conversational and helpful.";

            var userMessage = $@"Context:
{contextText}

Question: {prompt}

Please answer the question based on the context provided above.";

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.7,
                max_tokens = 1000
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation("Calling OpenAI chat API");
            
            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API Error ({StatusCode}): {Response}", 
                    response.StatusCode, responseBody.Substring(0, Math.Min(500, responseBody.Length)));
                
                return GenerateMockResponse(prompt, context);
            }
            
            var result = JsonSerializer.Deserialize<OpenAIChatResponse>(responseBody);

            if (result?.Choices == null || result.Choices.Count == 0)
            {
                _logger.LogError("Invalid response from OpenAI");
                return GenerateMockResponse(prompt, context);
            }

            _logger.LogInformation("✓ Successfully generated response from OpenAI");
            return result.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LLM response, using mock");
            return GenerateMockResponse(prompt, context);
        }
    }

    private string GenerateMockResponse(string prompt, List<string> context)
    {
        _logger.LogInformation("Generating mock response");
        
        if (!context.Any())
        {
            return "No relevant documents found. Please upload some documents first.";
        }
        
        // Try to extract specific answer
        var answer = ExtractSpecificAnswer(prompt, context);
        
        if (!string.IsNullOrEmpty(answer))
        {
            return answer;
        }
        
        // Fallback: return first part of context
        var firstContext = context[0];
        return firstContext.Length > 300 ? firstContext.Substring(0, 300) + "..." : firstContext;
    }

    private string ExtractSpecificAnswer(string prompt, List<string> contexts)
    {
        var promptLower = prompt.ToLower();
        
        // Check if asking about a specific step
        var stepMatch = System.Text.RegularExpressions.Regex.Match(promptLower, @"(?:step|#)\s*(\d+)");
        if (stepMatch.Success)
        {
            var stepNumber = stepMatch.Groups[1].Value;
            _logger.LogInformation($"Looking for step {stepNumber}");
            
            // Search through all contexts for this specific step
            foreach (var context in contexts)
            {
                // Pattern specifically for "# 8 Build UI..." format
                // Match "# {number} {content}" and stop before next "# {number}"
                var pattern = $@"#\s*{stepNumber}\s+([^#\n]+?)(?=\s*#\s*\d+|\.\s*#\s*\d+|$)";
                
                var match = System.Text.RegularExpressions.Regex.Match(
                    context, 
                    pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                if (match.Success && match.Groups[1].Value.Trim().Length > 0)
                {
                    var extracted = match.Groups[1].Value.Trim();
                    // Remove trailing periods and clean whitespace
                    extracted = extracted.TrimEnd('.', ' ');
                    _logger.LogInformation($"Successfully extracted step {stepNumber}: {extracted}");
                    return extracted;
                }
                
                // Fallback: try simpler pattern that gets everything after "# X " until newline
                pattern = $@"#\s*{stepNumber}\s+([^\n]+)";
                match = System.Text.RegularExpressions.Regex.Match(context, pattern);
                
                if (match.Success && match.Groups[1].Value.Trim().Length > 0)
                {
                    var extracted = match.Groups[1].Value.Trim();
                    extracted = extracted.TrimEnd('.', ' ');
                    _logger.LogInformation($"Extracted step {stepNumber} (fallback): {extracted}");
                    return extracted;
                }
            }
        }
        
        // Check if asking about a specific numbered item
        var numberMatch = System.Text.RegularExpressions.Regex.Match(promptLower, @"(?:what is|explain|describe).*?(\d+)");
        if (numberMatch.Success)
        {
            var number = numberMatch.Groups[1].Value;
            
            foreach (var context in contexts)
            {
                var pattern = $@"#\s*{number}\s+([^\n#]+)";
                var match = System.Text.RegularExpressions.Regex.Match(context, pattern);
                
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim(); // Return just the answer
                }
            }
        }
        
        return string.Empty;
    }

    private string? FindBestMatchingContext(string prompt, List<string> contexts)
    {
        // Extract key terms from the prompt
        var promptLower = prompt.ToLower();
        var keywords = new List<string>();
        
        // Look for step numbers
        var stepMatch = System.Text.RegularExpressions.Regex.Match(promptLower, @"step\s*(\d+)");
        if (stepMatch.Success)
        {
            var stepNumber = stepMatch.Groups[1].Value;
            keywords.Add($"# {stepNumber}");
            keywords.Add($"#{stepNumber}");
            keywords.Add($"step {stepNumber}");
            keywords.Add($"# step {stepNumber}");
        }
        
        // Look for other numbered items
        var numberMatch = System.Text.RegularExpressions.Regex.Match(promptLower, @"(\d+)");
        if (numberMatch.Success && !stepMatch.Success)
        {
            var number = numberMatch.Groups[1].Value;
            keywords.Add($"# {number}");
            keywords.Add($"#{number}");
        }
        
        // Look for specific words in the question
        var words = promptLower.Split(new[] { ' ', '?', '.', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3 && !new[] { "what", "how", "when", "where", "which", "this", "that", "the" }.Contains(w))
            .ToList();
        keywords.AddRange(words);
        
        // Score each context
        var scoredContexts = contexts.Select((ctx, idx) => new
        {
            Context = ctx,
            Index = idx,
            Score = keywords.Sum(kw => ctx.ToLower().Contains(kw) ? 10 : 0) + 
                    (keywords.Any(kw => ctx.ToLower().StartsWith(kw)) ? 20 : 0)
        }).OrderByDescending(x => x.Score).ToList();
        
        var best = scoredContexts.FirstOrDefault();
        if (best != null && best.Score > 0)
        {
            _logger.LogInformation($"Best matching context (score: {best.Score}): {best.Context.Substring(0, Math.Min(100, best.Context.Length))}");
            return best.Context;
        }
        
        return null;
    }

    private class OpenAIChatResponse
    {
        public List<Choice> Choices { get; set; } = new();
    }

    private class Choice
    {
        public Message Message { get; set; } = new();
    }

    private class Message
    {
        public string Content { get; set; } = "";
    }
}
