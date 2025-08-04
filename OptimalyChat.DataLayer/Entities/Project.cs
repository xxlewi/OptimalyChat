using System.ComponentModel.DataAnnotations;

namespace OptimalyChat.DataLayer.Entities;

/// <summary>
/// Represents an AI chat project with optional encryption
/// </summary>
public class Project : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this project uses encryption for messages
    /// </summary>
    public bool IsEncrypted { get; set; } = false;
    
    /// <summary>
    /// Level of encryption applied to this project
    /// </summary>
    public EncryptionLevel EncryptionLevel { get; set; } = EncryptionLevel.None;
    
    /// <summary>
    /// Encrypted encryption key - stored separately from data
    /// </summary>
    public string? EncryptionKeyId { get; set; }
    
    /// <summary>
    /// User who owns this project
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to user
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// All conversations in this project
    /// </summary>
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    
    /// <summary>
    /// Documents attached to this project
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    
    // Computed properties
    public int ConversationCount => Conversations?.Count ?? 0;
    public int MessageCount => Conversations?.Sum(c => c.Messages?.Count ?? 0) ?? 0;
    public string SecurityStatus => IsEncrypted 
        ? $"🔒 Encrypted ({EncryptionLevel})" 
        : "🔓 Not encrypted";
}

/// <summary>
/// Defines the level of encryption for a project
/// </summary>
public enum EncryptionLevel
{
    /// <summary>
    /// No encryption
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Only message content is encrypted
    /// </summary>
    MessagesOnly = 1,
    
    /// <summary>
    /// Messages and metadata are encrypted
    /// </summary>
    Full = 2
}