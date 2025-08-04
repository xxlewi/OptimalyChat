using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Models;

namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// Conversation service interface for managing chat conversations
/// </summary>
public interface IConversationService : IBaseService<ConversationDto>
{
    /// <summary>
    /// Get conversations for a project
    /// </summary>
    Task<PagedResult<ConversationDto>> GetProjectConversationsAsync(int projectId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a new conversation in a project
    /// </summary>
    Task<ConversationDto> CreateConversationAsync(int projectId, string title, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get conversation with messages
    /// </summary>
    Task<ConversationDto> GetConversationWithMessagesAsync(int conversationId, int messageLimit = 50, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update conversation title
    /// </summary>
    Task<ConversationDto> UpdateTitleAsync(int conversationId, string newTitle, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete conversation and all messages
    /// </summary>
    Task DeleteConversationAsync(int conversationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search conversations in a project
    /// </summary>
    Task<IEnumerable<ConversationDto>> SearchConversationsAsync(int projectId, string searchTerm, CancellationToken cancellationToken = default);
}