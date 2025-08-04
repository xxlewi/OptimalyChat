using System.ComponentModel.DataAnnotations;

namespace OptimalyChat.DataLayer.Entities;

/// <summary>
/// Represents a conversation thread within a project
/// </summary>
public class Conversation : BaseEntity
{
    [Required]
    [StringLength(500)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// The project this conversation belongs to
    /// </summary>
    public int ProjectId { get; set; }
    
    /// <summary>
    /// Navigation property to project
    /// </summary>
    public virtual Project Project { get; set; } = null!;
    
    /// <summary>
    /// Messages in this conversation
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    
    /// <summary>
    /// Last message timestamp for sorting
    /// </summary>
    public DateTime? LastMessageAt { get; set; }
    
    /// <summary>
    /// Total tokens used in this conversation
    /// </summary>
    public int TotalTokensUsed { get; set; }
    
    // Computed properties
    public int MessageCount => Messages?.Count ?? 0;
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) 
        ? $"Conversation {Id}" 
        : Title;
    public string LastMessagePreview => Messages?
        .OrderByDescending(m => m.CreatedAt)
        .FirstOrDefault()?.Content?.Take(100) + "..." ?? "No messages yet";
}