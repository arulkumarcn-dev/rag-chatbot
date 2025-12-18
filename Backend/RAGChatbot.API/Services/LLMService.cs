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
            
            var systemMessage = @"You are a precise AI assistant that provides EXACT answers from the provided context, along with helpful study resources.

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

ADDITIONAL STUDY SUPPORT:
12. After providing the answer, add a '📚 Study Tips:' section with:
    - Key concepts to understand
    - Related topics to explore
    - Suggested external resources (Wikipedia, educational sites, videos)
    - Practice questions or examples if applicable

Format your response as:
[EXACT ANSWER FROM CONTEXT]

📚 Study Tips:
- Key Concept: [main concept]
- Related Topics: [related areas]
- External Resources: [suggest 2-3 relevant search terms or links]
- Study Suggestion: [how to learn this better]

Be precise, complete, and helpful for learning.";

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
            // For large quizzes (>20 questions), generate in batches to avoid token limits
            if (questionCount > 20)
            {
                return await GenerateQuizInBatchesAsync(context, topic, questionCount);
            }

            // For small quizzes, generate in one go
            return await GenerateSingleBatchQuizAsync(context, topic, questionCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            throw;
        }
    }

    private async Task<Quiz> GenerateSingleBatchQuizAsync(string context, string topic, int questionCount)
    {
        var systemMessage = @"You are an expert quiz generator and educator. Create clear, well-formatted multiple-choice questions based on the provided context.
Generate questions that test understanding, not just memorization.
You can use your knowledge to provide comprehensive explanations.
Your response must be ONLY a valid JSON array - no extra text, no markdown, no explanations outside the JSON.";

        var userMessage = $@"Based on the following content, generate {questionCount} high-quality multiple-choice questions.

Content:
{context}

IMPORTANT: Respond with ONLY a JSON array. No markdown, no code blocks, no extra text.

Required JSON format:
[
  {{
    ""question"": ""Clear, specific question text?"",
    ""options"": [""First option"", ""Second option"", ""Third option"", ""Fourth option""],
    ""correctAnswerIndex"": 0,
    ""explanation"": ""Detailed explanation of the correct answer with additional context.""
  }}
]

Requirements:
1. Each question must have EXACTLY 4 distinct options
2. Options should be concise (not full sentences with labels)
3. Only one correct answer per question (index 0-3)
4. Questions cover different aspects of the content
5. Explanations are comprehensive and educational
6. Use the same language as the content
7. No duplicate questions or options
8. Make options plausible but clearly distinguishable

Respond with ONLY the JSON array, nothing else.";

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

    private async Task<Quiz> GenerateQuizInBatchesAsync(string context, string topic, int totalQuestions)
    {
        _logger.LogInformation("Generating {TotalQuestions} questions in batches", totalQuestions);

        var allQuestions = new List<QuizQuestion>();
        var batchSize = 20; // Generate 20 questions per batch
        var batches = (int)Math.Ceiling((double)totalQuestions / batchSize);

        // Split context into sections for diverse question generation
        var contextSections = SplitContextIntoSections(context, batches);

        for (int i = 0; i < batches; i++)
        {
            var questionsInBatch = (i == batches - 1) 
                ? totalQuestions - (batchSize * i) 
                : batchSize;

            var batchContext = contextSections[i % contextSections.Count];

            _logger.LogInformation("Generating batch {BatchNum}/{TotalBatches} ({QuestionCount} questions)",
                i + 1, batches, questionsInBatch);

            var batchQuiz = await GenerateSingleBatchQuizAsync(batchContext, topic, questionsInBatch);
            
            // Add questions with adjusted IDs
            foreach (var question in batchQuiz.Questions)
            {
                question.Id = allQuestions.Count + 1;
                allQuestions.Add(question);
            }

            // Small delay to avoid rate limits
            if (i < batches - 1)
            {
                await Task.Delay(500);
            }
        }

        var finalQuiz = new Quiz
        {
            Topic = topic,
            GeneratedAt = DateTime.UtcNow,
            Questions = allQuestions
        };

        _logger.LogInformation("Successfully generated {TotalQuestions} questions across {Batches} batches",
            allQuestions.Count, batches);

        return finalQuiz;
    }

    private List<string> SplitContextIntoSections(string context, int sectionCount)
    {
        // Split context into roughly equal sections for diverse coverage
        var paragraphs = context.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var sections = new List<string>();

        if (paragraphs.Length <= sectionCount)
        {
            // If we have fewer paragraphs than sections, use all content for each section
            for (int i = 0; i < sectionCount; i++)
            {
                sections.Add(context);
            }
        }
        else
        {
            var parasPerSection = paragraphs.Length / sectionCount;
            for (int i = 0; i < sectionCount; i++)
            {
                var start = i * parasPerSection;
                var count = (i == sectionCount - 1) 
                    ? paragraphs.Length - start 
                    : parasPerSection;

                var sectionParas = paragraphs.Skip(start).Take(count);
                sections.Add(string.Join("\n\n", sectionParas));
            }
        }

        return sections;
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
