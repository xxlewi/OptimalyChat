using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.ServiceLayer.Services;

/// <summary>
/// LM Studio API client implementation
/// </summary>
public class LMStudioClient : ILMStudioClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LMStudioClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public LMStudioClient(HttpClient httpClient, ILogger<LMStudioClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        // Configure base URL
        var baseUrl = _configuration["LMStudio:BaseUrl"] ?? "http://localhost:1234/v1";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Get available models from LM Studio
    /// </summary>
    public async Task<IEnumerable<LMStudioModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/models", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ModelsResponse>(content, _jsonOptions);
            
            return result?.Data ?? new List<LMStudioModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting models from LM Studio");
            return Array.Empty<LMStudioModel>();
        }
    }

    /// <summary>
    /// Create a chat completion
    /// </summary>
    public async Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.Stream = false; // Ensure streaming is disabled for this method
            
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, _jsonOptions);
            
            return result ?? throw new InvalidOperationException("Invalid response from LM Studio");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat completion");
            throw;
        }
    }

    /// <summary>
    /// Stream a chat completion
    /// </summary>
    public async IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(
        ChatCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true; // Enable streaming
        
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/chat/completions")
        {
            Content = content
        };
        
        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            
            if (string.IsNullOrWhiteSpace(line))
                continue;
                
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                
                if (data == "[DONE]")
                    break;
                    
                ChatCompletionChunk? chunk = null;
                try
                {
                    chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse streaming chunk: {Data}", data);
                    continue;
                }
                
                if (chunk != null)
                    yield return chunk;
            }
        }
    }

    /// <summary>
    /// Test connection to LM Studio
    /// </summary>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to LM Studio");
            return false;
        }
    }
    
    private class ModelsResponse
    {
        public List<LMStudioModel> Data { get; set; } = new();
    }
}