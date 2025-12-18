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
            
            var systemMessage = @"You are a precise AI assistant that provides EXACT answers from the provided context.

IMPORTANT RULES:
1. Extract and provide the EXACT answer from the context - do NOT paraphrase
2. If a section number (like 1.1, 2.3) is mentioned, find and return that COMPLETE section
3. For 'what is' questions, provide the complete definition/explanation from context
4. For numbered items, return the FULL content of that item
5. If asking about length, dimensions, or specifications, provide the EXACT value
6. Always include relevant details, not just summaries
7. Preserve all technical terms, numbers, and specifics exactly as in context
8. If context is in Tamil, Hindi, or other languages, preserve the original text
9. Cite which context number you're using (e.g., 'From Context 1:')
10. If the exact answer isn't in context, say 'The context does not contain this specific information'

Be precise, complete, and accurate - prioritize exactness over brevity.";

            var userMessage = $@"Context:
{contextText}

Question: {prompt}

Provide the EXACT, COMPLETE answer from the context above. Include all relevant details and specifications.";

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.2,
                max_tokens = 1500
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

    public async Task<Models.Quiz> GenerateQuizAsync(string context, string topic, int questionCount)
    {
        if (_useMockMode)
        {
            return GenerateMockQuiz(topic, questionCount);
        }

        try
        {
            var systemMessage = @"You are an expert quiz generator and educator. Create multiple-choice questions based on the provided context.
Generate questions that test understanding, not just memorization.
You can use your knowledge to provide comprehensive explanations that go beyond the immediate context.
Format your response as a valid JSON array of questions.";

            var userMessage = $@"Based on the following content, generate {questionCount} multiple-choice questions.

Content:
{context}

Please provide exactly {questionCount} questions in this JSON format:
[
  {{
    ""question"": ""Question text here?"",
    ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
    ""correctAnswerIndex"": 0,
    ""explanation"": ""Detailed explanation with additional context and real-world applications."",
    ""hint"": ""A helpful hint to guide thinking without revealing the answer."",
    ""externalReferences"": [""https://example.com/topic1"", ""https://wikipedia.org/topic""],
    ""studyTip"": ""Practical tip or mnemonic to remember this concept.""
  }}
]

IMPORTANT Requirements:
1. Questions cover different aspects of the content
2. Each question has 4 options
3. Only one correct answer per question
4. **Hint**: Provide a helpful hint that guides thinking without revealing the answer
5. **Explanation**: Comprehensive explanation including:
   - Why the correct answer is correct
   - Why other options are incorrect
   - Additional context from your knowledge
   - Real-world applications/examples
   - Related concepts to explore
6. **External References**: 2-3 relevant URLs for further reading (educational sites, Wikipedia, tutorials)
7. **Study Tip**: Practical advice, mnemonic, or memory technique
8. Questions in the same language as content (Tamil, English, etc.)
9. Make content thorough and educational";

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.7,
                max_tokens = 3000
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation("Calling OpenAI API for quiz generation");
            
            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API Error ({StatusCode}): {Response}", 
                    response.StatusCode, responseBody.Substring(0, Math.Min(500, responseBody.Length)));
                
                return GenerateMockQuiz(topic, questionCount);
            }
            
            var result = JsonSerializer.Deserialize<OpenAIChatResponse>(responseBody);
            var quizJson = result?.Choices?[0]?.Message?.Content ?? "[]";

            // Clean up the JSON response
            quizJson = quizJson.Trim();
            if (quizJson.StartsWith("```json"))
            {
                quizJson = quizJson.Substring(7);
            }
            if (quizJson.StartsWith("```"))
            {
                quizJson = quizJson.Substring(3);
            }
            if (quizJson.EndsWith("```"))
            {
                quizJson = quizJson.Substring(0, quizJson.Length - 3);
            }
            quizJson = quizJson.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var questions = JsonSerializer.Deserialize<List<QuizQuestionDto>>(quizJson, options);

            if (questions == null || questions.Count == 0)
            {
                _logger.LogError("Failed to parse quiz questions from OpenAI response");
                return GenerateMockQuiz(topic, questionCount);
            }

            var quiz = new Models.Quiz
            {
                Topic = topic,
                Questions = questions.Select((q, index) => new Models.QuizQuestion
                {
                    Id = index + 1,
                    Question = q.Question,
                    Options = q.Options,
                    CorrectAnswerIndex = q.CorrectAnswerIndex,
                    Explanation = q.Explanation,
                    Hint = q.Hint,
                    ExternalReferences = q.ExternalReferences ?? new List<string>(),
                    StudyTip = q.StudyTip
                }).ToList(),
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("✓ Successfully generated quiz with {Count} questions", quiz.Questions.Count);
            return quiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz, using mock");
            return GenerateMockQuiz(topic, questionCount);
        }
    }

    private Models.Quiz GenerateMockQuiz(string topic, int questionCount)
    {
        _logger.LogInformation("Generating mock quiz for topic: {Topic}", topic);
        
        var questions = new List<Models.QuizQuestion>();
        
        for (int i = 1; i <= questionCount; i++)
        {
            questions.Add(new Models.QuizQuestion
            {
                Id = i,
                Question = $"Sample question {i} about {topic}?",
                Options = new List<string>
                {
                    $"Option A for question {i}",
                    $"Option B for question {i}",
                    $"Option C for question {i}",
                    $"Option D for question {i}"
                },
                CorrectAnswerIndex = i % 4,
                Explanation = $"This is a mock explanation for question {i}. In a real scenario with OpenAI API configured, you would get detailed explanations with external knowledge and practical examples.",
                Hint = $"Think about the key concepts related to {topic}.",
                ExternalReferences = new List<string> 
                { 
                    "https://en.wikipedia.org",
                    "https://www.khanacademy.org"
                },
                StudyTip = "Review the key concepts and practice regularly."
            });
        }

        return new Models.Quiz
        {
            Topic = topic,
            Questions = questions,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private class QuizQuestionDto
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; } = "";
        public string Hint { get; set; } = "";
        public List<string>? ExternalReferences { get; set; }
        public string StudyTip { get; set; } = "";
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
