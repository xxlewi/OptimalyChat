using System.ComponentModel.DataAnnotations;

namespace OptimalyChat.PresentationLayer.ViewModels;

public class ProfileViewModel
{
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Jméno")]
    public string? FirstName { get; set; }

    [Display(Name = "Příjmení")]
    public string? LastName { get; set; }

    [Display(Name = "Registrován")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Poslední přihlášení")]
    public DateTime? LastLoginAt { get; set; }

    public string FullName => 
        !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName) 
            ? $"{FirstName} {LastName}".Trim() 
            : Email;
}