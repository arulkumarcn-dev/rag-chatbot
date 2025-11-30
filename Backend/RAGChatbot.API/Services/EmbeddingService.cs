using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RAGChatbot.API.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public EmbeddingService(IConfiguration configuration, ILogger<EmbeddingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
        _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new Exception("OpenAI API Key not configured");
        _model = _configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var embeddings = await GenerateBatchEmbeddingsAsync(new List<string> { text });
            return embeddings.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding");
            throw;
        }
    }

    public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts)
    {
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

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/embeddings",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API Error ({StatusCode}): {Response}", response.StatusCode, responseBody);
                throw new Exception($"OpenAI API returned {response.StatusCode}: {responseBody}");
            }
            
            var result = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseBody);

            if (result?.Data == null)
                throw new Exception("Invalid response from OpenAI API");

            return result.Data
                .OrderBy(d => d.Index)
                .Select(d => d.Embedding)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch embeddings");
            throw;
        }
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
