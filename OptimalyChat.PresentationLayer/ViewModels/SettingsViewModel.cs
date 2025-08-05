using System.ComponentModel.DataAnnotations;
using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.PresentationLayer.ViewModels;

/// <summary>
/// View model for AI settings page
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    /// <summary>
    /// List of available AI models
    /// </summary>
    public List<AIModelDto> Models { get; set; } = new();
    
    /// <summary>
    /// Current LM Studio configuration
    /// </summary>
    public LMStudioSettingsViewModel LMStudioSettings { get; set; } = new();
    
    /// <summary>
    /// Whether LM Studio is connected
    /// </summary>
    public bool IsLMStudioConnected { get; set; }
    
    /// <summary>
    /// Connection status message
    /// </summary>
    public string? ConnectionStatus { get; set; }
}

/// <summary>
/// View model for LM Studio settings
/// </summary>
public class LMStudioSettingsViewModel
{
    [Required(ErrorMessage = "Server URL is required")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    [Display(Name = "LM Studio Server URL")]
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";
    
    [Display(Name = "Timeout (seconds)")]
    [Range(10, 600, ErrorMessage = "Timeout must be between 10 and 600 seconds")]
    public int TimeoutSeconds { get; set; } = 120;
}

/// <summary>
/// View model for creating/editing AI model
/// </summary>
public class AIModelEditViewModel : BaseViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Model name is required")]
    [StringLength(200, ErrorMessage = "Model name must be less than 200 characters")]
    [Display(Name = "Model Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Model ID is required")]
    [StringLength(200, ErrorMessage = "Model ID must be less than 200 characters")]
    [Display(Name = "Model ID")]
    public string ModelId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Provider is required")]
    [StringLength(100, ErrorMessage = "Provider must be less than 100 characters")]
    [Display(Name = "Provider")]
    public string Provider { get; set; } = "LMStudio";
    
    [Required(ErrorMessage = "Endpoint is required")]
    [StringLength(500, ErrorMessage = "Endpoint must be less than 500 characters")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    [Display(Name = "API Endpoint")]
    public string Endpoint { get; set; } = string.Empty;
    
    [Display(Name = "API Key")]
    [StringLength(500, ErrorMessage = "API Key must be less than 500 characters")]
    public string? ApiKey { get; set; }
    
    [Display(Name = "Max Tokens")]
    [Range(1, 1000000, ErrorMessage = "Max tokens must be between 1 and 1,000,000")]
    public int MaxTokens { get; set; } = 4096;
    
    [Display(Name = "Temperature")]
    [Range(0, 2, ErrorMessage = "Temperature must be between 0 and 2")]
    public double Temperature { get; set; } = 0.7;
    
    [Display(Name = "Cost per 1K Input Tokens")]
    [Range(0, 100, ErrorMessage = "Cost must be between 0 and 100")]
    public decimal? CostPer1KInput { get; set; }
    
    [Display(Name = "Cost per 1K Output Tokens")]
    [Range(0, 100, ErrorMessage = "Cost must be between 0 and 100")]
    public decimal? CostPer1KOutput { get; set; }
    
    [Display(Name = "Is Default Model")]
    public bool IsDefault { get; set; }
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}