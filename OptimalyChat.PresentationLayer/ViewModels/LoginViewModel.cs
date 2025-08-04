using System.ComponentModel.DataAnnotations;

namespace OptimalyChat.PresentationLayer.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Heslo je povinné")]
    [DataType(DataType.Password)]
    [Display(Name = "Heslo")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Zapamatovat si mě")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}