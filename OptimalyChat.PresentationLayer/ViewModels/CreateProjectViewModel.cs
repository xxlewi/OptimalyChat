using System.ComponentModel.DataAnnotations;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.PresentationLayer.ViewModels;

/// <summary>
/// View model for creating a new project
/// </summary>
public class CreateProjectViewModel : BaseViewModel
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Project name must be between 1 and 200 characters")]
    [Display(Name = "Project Name")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [Display(Name = "Enable Encryption")]
    public bool IsEncrypted { get; set; } = false;
    
    [Display(Name = "Encryption Level")]
    public EncryptionLevel EncryptionLevel { get; set; } = EncryptionLevel.None;
}