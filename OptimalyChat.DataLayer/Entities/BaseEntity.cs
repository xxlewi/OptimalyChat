using OptimalyChat.DataLayer.Interfaces;

namespace OptimalyChat.DataLayer.Entities;

/// <summary>
/// Base entity with comprehensive audit trail and soft delete functionality
/// Supports tracking who created, updated, and deleted records with timestamps
/// </summary>
public abstract class BaseEntity : IBaseEntity<int>
{
    /// <summary>
    /// Primary key identifier
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// UTC timestamp when record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// UTC timestamp when record was last updated (null if never updated)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// User identifier who created this record
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// User identifier who last updated this record
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Soft delete flag - when true, record is logically deleted but preserved in database
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// UTC timestamp when record was soft deleted (null if never deleted)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// User identifier who soft deleted this record
    /// </summary>
    public string? DeletedBy { get; set; }
    
    /// <summary>
    /// Computed property - is this record active (not deleted)
    /// </summary>
    public bool IsActive => !IsDeleted;
    
    /// <summary>
    /// Computed property - how long ago was this record created
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
    
    /// <summary>
    /// Computed property - formatted creation date for display (converted to local time)
    /// </summary>
    public string CreatedAtDisplay => CreatedAt.Kind == DateTimeKind.Utc 
        ? CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm") 
        : CreatedAt.ToString("dd.MM.yyyy HH:mm");
    
    /// <summary>
    /// Computed property - formatted update date for display (converted to local time)
    /// </summary>
    public string? UpdatedAtDisplay => UpdatedAt?.Kind == DateTimeKind.Utc 
        ? UpdatedAt?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") 
        : UpdatedAt?.ToString("dd.MM.yyyy HH:mm");
    
    /// <summary>
    /// Computed property - last modified date (UpdatedAt or CreatedAt if never updated)
    /// </summary>
    public DateTime LastModified => UpdatedAt ?? CreatedAt;
    
    /// <summary>
    /// Computed property - last modified by user (UpdatedBy or CreatedBy if never updated)
    /// </summary>
    public string? LastModifiedBy => UpdatedBy ?? CreatedBy;
    
    /// <summary>
    /// Computed property - local creation date for display
    /// </summary>
    public DateTime CreatedAtLocal => CreatedAt.Kind == DateTimeKind.Utc ? CreatedAt.ToLocalTime() : CreatedAt;
    
    /// <summary>
    /// Computed property - local update date for display
    /// </summary>
    public DateTime? UpdatedAtLocal => UpdatedAt?.Kind == DateTimeKind.Utc ? UpdatedAt?.ToLocalTime() : UpdatedAt;
    
    /// <summary>
    /// Computed property - formatted creation date with Czech locale
    /// </summary>
    public string CreatedAtDisplayFull => CreatedAtLocal.ToString("dddd, dd. MMMM yyyy HH:mm", new System.Globalization.CultureInfo("cs-CZ"));
    
    /// <summary>
    /// Computed property - relative time display (před 5 minutami, včera, atd.)
    /// </summary>
    public string CreatedAtRelative
    {
        get
        {
            var diff = DateTime.Now - CreatedAtLocal;
            if (diff.TotalMinutes < 1) return "Právě teď";
            if (diff.TotalMinutes < 60) return $"Před {(int)diff.TotalMinutes} minutami";
            if (diff.TotalHours < 24) return $"Před {(int)diff.TotalHours} hodinami";
            if (diff.TotalDays < 7) return $"Před {(int)diff.TotalDays} dny";
            return CreatedAtDisplay;
        }
    }
}