using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptimalyChat.DataLayer.Entities;

/// <summary>
/// Represents a message in a conversation
/// </summary>
public class Message : BaseEntity
{
    /// <summary>
    /// The conversation this message belongs to
    /// </summary>
    public int ConversationId { get; set; }
    
    /// <summary>
    /// Navigation property to conversation
    /// </summary>
    public virtual Conversation Conversation { get; set; } = null!;
    
    /// <summary>
    /// Role of the message sender (user or assistant)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Plain text content (null if encrypted)
    /// </summary>
    [Column(TypeName = "text")]
    public string? Content { get; set; }
    
    /// <summary>
    /// Encrypted content (if project uses encryption)
    /// </summary>
    [Column(TypeName = "text")]
    public string? EncryptedContent { get; set; }
    
    /// <summary>
    /// Nonce for AES-GCM encryption
    /// </summary>
    public string? Nonce { get; set; }
    
    /// <summary>
    /// Authentication tag for AES-GCM
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Vector embedding for similarity search
    /// </summary>
    [Column(TypeName = "bytea")]
    public byte[]? Embedding { get; set; }
    
    /// <summary>
    /// Whether this message has been indexed for search
    /// </summary>
    public bool IsIndexed { get; set; } = false;
    
    /// <summary>
    /// Number of tokens in this message
    /// </summary>
    public int TokenCount { get; set; }
    
    /// <summary>
    /// Model used to generate this message (for assistant messages)
    /// </summary>
    [StringLength(100)]
    public string? Model { get; set; }
    
    /// <summary>
    /// Time taken to generate response (for assistant messages)
    /// </summary>
    public int? ResponseTimeMs { get; set; }
    
    // Computed properties
    public bool IsUserMessage => Role.Equals("user", StringComparison.OrdinalIgnoreCase);
    public bool IsAssistantMessage => Role.Equals("assistant", StringComparison.OrdinalIgnoreCase);
    public bool IsEncrypted => !string.IsNullOrEmpty(EncryptedContent);
    public string DisplayContent => Content ?? (IsEncrypted ? "[Encrypted Message]" : "[No Content]");
    public string ShortContent => DisplayContent.Length > 100 
        ? DisplayContent.Substring(0, 100) + "..." 
        : DisplayContent;
}