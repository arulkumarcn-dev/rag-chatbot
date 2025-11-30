using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RAGChatbot.API.Services;

public class RobustEmbeddingService : IEmbeddingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RobustEmbeddingService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly bool _useMockMode;

    public RobustEmbeddingService(IConfiguration configuration, ILogger<RobustEmbeddingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        _apiKey = _configuration["OpenAI:ApiKey"] ?? "";
        _model = _configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        
        // Check if we should use mock mode
        _useMockMode = string.IsNullOrEmpty(_apiKey) || 
                       _apiKey == "your-openai-api-key-here" ||
                       !_apiKey.StartsWith("sk-");
        
        if (_useMockMode)
        {
            _logger.LogWarning("⚠️ Using MOCK embeddings (OpenAI API key not configured or invalid)");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _logger.LogInformation("✓ Using OpenAI embeddings");
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (_useMockMode)
        {
            return GenerateMockEmbedding(text);
        }

        try
        {
            var embeddings = await GenerateBatchEmbeddingsAsync(new List<string> { text });
            return embeddings.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding, falling back to mock");
            return GenerateMockEmbedding(text);
        }
    }

    public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts)
    {
        if (_useMockMode)
        {
            _logger.LogInformation("Generating {Count} mock embeddings", texts.Count);
            return texts.Select(t => GenerateMockEmbedding(t)).ToList();
        }

        try
        {
            var request = new
            {
                input = texts,
                model = _model
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation("Calling OpenAI API for {Count} embeddings", texts.Count);
            
            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/embeddings",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API Error ({StatusCode}): {Response}", 
                    response.StatusCode, responseBody.Substring(0, Math.Min(500, responseBody.Length)));
                
                // Try to parse error message
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    if (errorObj.TryGetProperty("error", out var error))
                    {
                        var errorMsg = error.GetProperty("message").GetString();
                        _logger.LogError("OpenAI Error Message: {Error}", errorMsg);
                        
                        if (errorMsg.Contains("API key") || errorMsg.Contains("Incorrect"))
                        {
                            _logger.LogError("⚠️ Invalid API key! Switching to MOCK mode for this session.");
                            return texts.Select(t => GenerateMockEmbedding(t)).ToList();
                        }
                    }
                }
                catch { }
                
                throw new Exception($"OpenAI API returned {response.StatusCode}. Check API key and account status.");
            }
            
            var result = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseBody);

            if (result?.Data == null)
            {
                _logger.LogError("Invalid response structure from OpenAI");
                throw new Exception("Invalid response from OpenAI API");
            }

            _logger.LogInformation("✓ Successfully generated {Count} embeddings from OpenAI", texts.Count);
            
            return result.Data
                .OrderBy(d => d.Index)
                .Select(d => d.Embedding)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling OpenAI API, using mock embeddings");
            return texts.Select(t => GenerateMockEmbedding(t)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch embeddings from OpenAI, using mock embeddings");
            return texts.Select(t => GenerateMockEmbedding(t)).ToList();
        }
    }

    private float[] GenerateMockEmbedding(string text)
    {
        // Generate deterministic mock embeddings
        const int dimension = 1536;
        var embedding = new float[dimension];
        
        // Use hash of text for deterministic generation
        var hash = text.GetHashCode();
        var random = new Random(hash);
        
        for (int i = 0; i < dimension; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }
        
        // Normalize
        var magnitude = Math.Sqrt(embedding.Sum(x => x * x));
        for (int i = 0; i < dimension; i++)
        {
            embedding[i] /= (float)magnitude;
        }
        
        return embedding;
    }

    private class OpenAIEmbeddingResponse
    {
        public List<EmbeddingData> Data { get; set; } = new();
    }

    private class EmbeddingData
    {
        public int Index { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
