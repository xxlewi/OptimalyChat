namespace OptimalyChat.ServiceLayer.DTOs;

/// <summary>
/// Message data transfer object
/// </summary>
public class MessageDto : BaseDto
{
    public int ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsIndexed { get; set; }
    public int TokenCount { get; set; }
    public string? Model { get; set; }
    public int? ResponseTimeMs { get; set; }
    public bool IsUserMessage { get; set; }
    public bool IsAssistantMessage { get; set; }
    public bool IsEncrypted { get; set; }
    public string DisplayContent { get; set; } = string.Empty;
    public string ShortContent { get; set; } = string.Empty;
}