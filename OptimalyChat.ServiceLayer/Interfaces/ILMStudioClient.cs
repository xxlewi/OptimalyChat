namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// LM Studio API client interface
/// </summary>
public interface ILMStudioClient
{
    /// <summary>
    /// Get available models from LM Studio
    /// </summary>
    Task<IEnumerable<LMStudioModel>> GetModelsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a chat completion
    /// </summary>
    Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stream a chat completion
    /// </summary>
    IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Test connection to LM Studio
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get loaded models from LM Studio
    /// </summary>
    Task<IEnumerable<LMStudioLoadedModel>> GetLoadedModelsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// LM Studio model information
/// </summary>
public class LMStudioModel
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = "model";
    public long Created { get; set; }
    public string OwnedBy { get; set; } = "local";
}

/// <summary>
/// Chat completion request
/// </summary>
public class ChatCompletionRequest
{
    public string Model { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2048;
    public bool Stream { get; set; } = false;
    public double TopP { get; set; } = 0.95;
    public int? Seed { get; set; }
}

/// <summary>
/// Chat message
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Chat completion response
/// </summary>
public class ChatCompletionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = "chat.completion";
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public List<ChatChoice> Choices { get; set; } = new();
    public ChatUsage? Usage { get; set; }
}

/// <summary>
/// Chat choice
/// </summary>
public class ChatChoice
{
    public int Index { get; set; }
    public ChatMessage Message { get; set; } = new();
    public string? FinishReason { get; set; }
}

/// <summary>
/// Token usage information
/// </summary>
public class ChatUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

/// <summary>
/// Streaming chunk
/// </summary>
public class ChatCompletionChunk
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = "chat.completion.chunk";
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public List<ChatChunkChoice> Choices { get; set; } = new();
}

/// <summary>
/// Streaming chunk choice
/// </summary>
public class ChatChunkChoice
{
    public int Index { get; set; }
    public ChatChunkDelta Delta { get; set; } = new();
    public string? FinishReason { get; set; }
}

/// <summary>
/// Streaming chunk delta
/// </summary>
public class ChatChunkDelta
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

/// <summary>
/// LM Studio loaded model information
/// </summary>
public class LMStudioLoadedModel
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = "model";
    public string OwnedBy { get; set; } = "organization_owner";
    public List<object> Permission { get; set; } = new();
    public int Created { get; set; }
    public string State { get; set; } = "not-loaded";
    public double? EstimatedVram { get; set; }
    public double? EstimatedVramGB { get; set; }
}