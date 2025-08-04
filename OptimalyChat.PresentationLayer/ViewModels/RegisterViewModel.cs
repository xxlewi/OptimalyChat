using System.ComponentModel.DataAnnotations;

namespace OptimalyChat.PresentationLayer.ViewModels;

public class RegisterViewModel
{
    [Display(Name = "Jméno")]
    [StringLength(100, ErrorMessage = "Jméno může mít maximálně {1} znaků")]
    public string? FirstName { get; set; }

    [Display(Name = "Příjmení")]
    [StringLength(100, ErrorMessage = "Příjmení může mít maximálně {1} znaků")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Email je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Heslo je povinné")]
    [StringLength(100, ErrorMessage = "Heslo musí mít alespoň {2} znaků", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Heslo")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Potvrdit heslo")]
    [Compare("Password", ErrorMessage = "Hesla se neshodují")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Souhlasím s podmínkami")]
    public bool AgreeToTerms { get; set; }
}