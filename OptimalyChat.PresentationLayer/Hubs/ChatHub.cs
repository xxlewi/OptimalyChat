using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OptimalyChat.ServiceLayer.Interfaces;
using System.Runtime.CompilerServices;

namespace OptimalyChat.PresentationLayer.Hubs;

/// <summary>
/// SignalR hub for real-time chat communication
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IAIService _aiService;
    private readonly IProjectService _projectService;
    private readonly IConversationService _conversationService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IAIService aiService,
        IProjectService projectService,
        IConversationService conversationService,
        ILogger<ChatHub> logger)
    {
        _aiService = aiService;
        _projectService = projectService;
        _conversationService = conversationService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a project room for receiving updates
    /// </summary>
    public async Task JoinProject(int projectId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated");
        }

        // Verify user has access to project
        var project = await _projectService.GetByIdAsync(projectId);
        if (project == null || project.UserId != userId)
        {
            throw new HubException("Access denied to project");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
        await Clients.Caller.SendAsync("JoinedProject", projectId);
    }

    /// <summary>
    /// Leave a project room
    /// </summary>
    public async Task LeaveProject(int projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
        await Clients.Caller.SendAsync("LeftProject", projectId);
    }

    /// <summary>
    /// Join a conversation room for receiving updates
    /// </summary>
    public async Task JoinConversation(int conversationId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated");
        }

        // Verify user has access to conversation
        var conversation = await _conversationService.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            throw new HubException("Conversation not found");
        }

        var project = await _projectService.GetByIdAsync(conversation.ProjectId);
        if (project == null || project.UserId != userId)
        {
            throw new HubException("Access denied to conversation");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        await Clients.Caller.SendAsync("JoinedConversation", conversationId);
    }

    /// <summary>
    /// Leave a conversation room
    /// </summary>
    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        await Clients.Caller.SendAsync("LeftConversation", conversationId);
    }

    /// <summary>
    /// Send a message and stream the AI response
    /// </summary>
    public async IAsyncEnumerable<string> SendMessage(
        int projectId, 
        int conversationId, 
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated");
        }

        // Verify access
        var project = await _projectService.GetByIdAsync(projectId, cancellationToken);
        if (project == null || project.UserId != userId)
        {
            throw new HubException("Access denied to project");
        }

        // Notify others that AI is typing
        await Clients.OthersInGroup($"conversation-{conversationId}")
            .SendAsync("AITyping", conversationId, true, cancellationToken);

        try
        {
            // Stream the response
            await foreach (var chunk in _aiService.StreamResponseAsync(projectId, conversationId, message, cancellationToken))
            {
                yield return chunk;
                
                // Also send to other clients in the conversation
                await Clients.OthersInGroup($"conversation-{conversationId}")
                    .SendAsync("ReceiveMessageChunk", conversationId, chunk, cancellationToken);
            }
        }
        finally
        {
            // Notify others that AI stopped typing
            await Clients.OthersInGroup($"conversation-{conversationId}")
                .SendAsync("AITyping", conversationId, false, cancellationToken);
        }
    }

    /// <summary>
    /// Create a new conversation
    /// </summary>
    public async Task<int> CreateConversation(int projectId, string title)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated");
        }

        // Verify access
        var project = await _projectService.GetByIdAsync(projectId);
        if (project == null || project.UserId != userId)
        {
            throw new HubException("Access denied to project");
        }

        var conversation = await _conversationService.CreateConversationAsync(projectId, title);
        
        // Notify all clients in the project
        await Clients.Group($"project-{projectId}")
            .SendAsync("ConversationCreated", conversation);

        return conversation.Id;
    }

    /// <summary>
    /// Update conversation title
    /// </summary>
    public async Task UpdateConversationTitle(int conversationId, string newTitle)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated");
        }

        // Verify access
        var conversation = await _conversationService.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            throw new HubException("Conversation not found");
        }

        var project = await _projectService.GetByIdAsync(conversation.ProjectId);
        if (project == null || project.UserId != userId)
        {
            throw new HubException("Access denied to conversation");
        }

        var updated = await _conversationService.UpdateTitleAsync(conversationId, newTitle);
        
        // Notify all clients in the conversation
        await Clients.Group($"conversation-{conversationId}")
            .SendAsync("ConversationTitleUpdated", conversationId, newTitle);
    }

    /// <summary>
    /// Test LM Studio connection
    /// </summary>
    public async Task<bool> TestConnection()
    {
        return await _aiService.TestConnectionAsync();
    }
}