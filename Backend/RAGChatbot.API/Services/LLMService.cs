using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RAGChatbot.API.Models;

namespace RAGChatbot.API.Services;

public class LLMService : ILLMService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LLMService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public LLMService(IConfiguration configuration, ILogger<LLMService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
        _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new Exception("OpenAI API Key not configured");
        _model = _configuration["OpenAI:ChatModel"] ?? "gpt-4";
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GenerateResponseAsync(string prompt, List<string> context)
    {
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
8. If context is in Tamil, Hindi, Telugu, or other languages, preserve the original text exactly
9. Cite which context number you're using (e.g., 'From Context 1:')
10. If the exact answer isn't in context, say 'The context does not contain this specific information'
11. Respond in the same language as the question when appropriate

Be precise, complete, and accurate - prioritize exactness over brevity.";

            var userMessage = $@"Context:
{contextText}

Question: {prompt}

Provide the EXACT, COMPLETE answer from the context above. Include all relevant details and specifications. If the question is in a non-English language, respond in that language.";

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

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API Error ({StatusCode}): {Response}", response.StatusCode, responseBody);
                throw new Exception($"OpenAI API returned {response.StatusCode}: {responseBody}");
            }
            
            var result = JsonSerializer.Deserialize<OpenAIChatResponse>(responseBody);

            if (result?.Choices == null || result.Choices.Count == 0)
                throw new Exception("Invalid response from OpenAI API");

            return result.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LLM response");
            throw;
        }
    }

    private class OpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new();
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; } = new();
    }

    private class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public async Task<Quiz> GenerateQuizAsync(string context, string topic, int questionCount)
    {
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
    ""explanation"": ""Detailed explanation of why this is the correct answer. You may include additional context from your knowledge base to help the learner understand better.""
  }}
]

Ensure:
1. Questions cover different aspects of the content
2. Each question has 4 options
3. Only one correct answer per question
4. Explanations are comprehensive and educational:
   - Explain why the correct answer is correct
   - You may reference external knowledge or related concepts
   - Provide additional context that helps understanding
   - Include practical examples or real-world applications when relevant
   - Mention related topics or concepts the learner should explore
5. Questions are in the same language as the content
6. Make explanations thorough enough that learners gain deeper understanding beyond just the answer";

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

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API Error ({StatusCode}): {Response}", response.StatusCode, responseBody);
                throw new Exception($"OpenAI API returned {response.StatusCode}");
            }
            
            var result = JsonSerializer.Deserialize<OpenAIChatResponse>(responseBody);
            var quizJson = result?.Choices?[0]?.Message?.Content ?? "[]";

            // Clean up the JSON (remove markdown code blocks if present)
            quizJson = quizJson.Trim();
            if (quizJson.StartsWith("```json"))
                quizJson = quizJson.Substring(7);
            if (quizJson.StartsWith("```"))
                quizJson = quizJson.Substring(3);
            if (quizJson.EndsWith("```"))
                quizJson = quizJson.Substring(0, quizJson.Length - 3);
            quizJson = quizJson.Trim();

            // Parse the JSON
            var questions = JsonSerializer.Deserialize<List<QuizQuestionDto>>(quizJson) ?? new List<QuizQuestionDto>();

            var quiz = new Quiz
            {
                Topic = topic,
                GeneratedAt = DateTime.UtcNow,
                Questions = questions.Select((q, index) => new QuizQuestion
                {
                    Id = index + 1,
                    Question = q.Question,
                    Options = q.Options,
                    CorrectAnswerIndex = q.CorrectAnswerIndex,
                    Explanation = q.Explanation
                }).ToList()
            };

            return quiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            throw;
        }
    }

    private class QuizQuestionDto
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;
        
        [JsonPropertyName("options")]
        public List<string> Options { get; set; } = new();
        
        [JsonPropertyName("correctAnswerIndex")]
        public int CorrectAnswerIndex { get; set; }
        
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
    }
}
