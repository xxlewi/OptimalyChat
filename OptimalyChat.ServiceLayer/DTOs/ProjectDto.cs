using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.ServiceLayer.DTOs;

/// <summary>
/// Project data transfer object
/// </summary>
public class ProjectDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
    public EncryptionLevel EncryptionLevel { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ConversationCount { get; set; }
    public int MessageCount { get; set; }
    public string SecurityStatus { get; set; } = string.Empty;
    
    // Navigation properties
    public List<ConversationDto> Conversations { get; set; } = new();
}