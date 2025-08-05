namespace OptimalyChat.ServiceLayer.DTOs;

/// <summary>
/// AI Model data transfer object
/// </summary>
public class AIModelDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public double Temperature { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public decimal? CostPer1KInput { get; set; }
    public decimal? CostPer1KOutput { get; set; }
    public bool IsLocalModel { get; set; }
    public bool RequiresApiKey { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this model is currently loaded in LM Studio
    /// </summary>
    public bool IsLoadedInLMStudio { get; set; }
}