namespace OptimalyChat.ServiceLayer.DTOs;

/// <summary>
/// Conversation data transfer object
/// </summary>
public class ConversationDto : BaseDto
{
    public string Title { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int TotalTokensUsed { get; set; }
    public int MessageCount { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public string LastMessagePreview { get; set; } = string.Empty;
    
    // Navigation properties
    public List<MessageDto> Messages { get; set; } = new();
}