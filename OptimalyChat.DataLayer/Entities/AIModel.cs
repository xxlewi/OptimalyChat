using System.ComponentModel.DataAnnotations;

namespace OptimalyChat.DataLayer.Entities;

/// <summary>
/// Represents an AI model configuration
/// </summary>
public class AIModel : BaseEntity
{
    /// <summary>
    /// Display name of the model
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Model identifier used in API calls
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ModelId { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider of the model (LMStudio, OpenAI, etc.)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Provider { get; set; } = "LMStudio";
    
    /// <summary>
    /// API endpoint for this model
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Endpoint { get; set; } = "http://localhost:1234/v1";
    
    /// <summary>
    /// API key if required (encrypted)
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Maximum context length in tokens
    /// </summary>
    public int MaxTokens { get; set; } = 4096;
    
    /// <summary>
    /// Default temperature for generation
    /// </summary>
    public double Temperature { get; set; } = 0.7;
    
    /// <summary>
    /// Whether this is the default model
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Whether this model is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Cost per 1K input tokens (for tracking)
    /// </summary>
    public decimal? CostPer1KInput { get; set; }
    
    /// <summary>
    /// Cost per 1K output tokens (for tracking)
    /// </summary>
    public decimal? CostPer1KOutput { get; set; }
    
    /// <summary>
    /// Model capabilities as JSON
    /// </summary>
    public string? Capabilities { get; set; }
    
    // Computed properties
    public bool IsLocalModel => Provider == "LMStudio" || Endpoint.Contains("localhost");
    public bool RequiresApiKey => !IsLocalModel && !string.IsNullOrEmpty(ApiKey);
    public string DisplayName => $"{Name} ({Provider})";
}