using System.ComponentModel.DataAnnotations;

namespace OptimalyChat.DataLayer.Entities;

/// <summary>
/// Represents a document attached to a project for context
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// The project this document belongs to
    /// </summary>
    public int ProjectId { get; set; }
    
    /// <summary>
    /// Navigation property to project
    /// </summary>
    public virtual Project Project { get; set; } = null!;
    
    /// <summary>
    /// Original filename
    /// </summary>
    [Required]
    [StringLength(500)]
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME type of the document
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Path to the stored file
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string StoragePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Extracted text content for indexing
    /// </summary>
    public string? ExtractedText { get; set; }
    
    /// <summary>
    /// Whether this document has been indexed
    /// </summary>
    public bool IsIndexed { get; set; } = false;
    
    /// <summary>
    /// Number of chunks this document was split into
    /// </summary>
    public int ChunkCount { get; set; }
    
    // Computed properties
    public string FileSizeDisplay => FileSize switch
    {
        < 1024 => $"{FileSize} B",
        < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{FileSize / (1024.0 * 1024.0):F1} MB",
        _ => $"{FileSize / (1024.0 * 1024.0 * 1024.0):F1} GB"
    };
    
    public string FileExtension => Path.GetExtension(FileName).ToLowerInvariant();
    public bool IsTextDocument => ContentType.StartsWith("text/") || 
                                  FileExtension is ".txt" or ".md" or ".json" or ".xml" or ".csv";
    public bool IsPdfDocument => ContentType == "application/pdf" || FileExtension == ".pdf";
}