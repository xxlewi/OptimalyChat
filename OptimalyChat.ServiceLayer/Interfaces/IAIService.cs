using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// AI service interface for chat operations
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Get AI response for a message
    /// </summary>
    Task<MessageDto> GetResponseAsync(int projectId, int conversationId, string message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stream AI response for a message
    /// </summary>
    IAsyncEnumerable<string> StreamResponseAsync(int projectId, int conversationId, string message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get relevant context from project history
    /// </summary>
    Task<IEnumerable<MessageDto>> GetRelevantContextAsync(int projectId, string query, int topK = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Test AI connection
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get available AI models
    /// </summary>
    Task<IEnumerable<AIModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set default AI model
    /// </summary>
    Task SetDefaultModelAsync(int modelId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Toggle model active status
    /// </summary>
    Task ToggleModelStatusAsync(int modelId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete AI model
    /// </summary>
    Task DeleteModelAsync(int modelId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sync models from LM Studio
    /// </summary>
    Task SyncModelsFromLMStudioAsync(CancellationToken cancellationToken = default);
}