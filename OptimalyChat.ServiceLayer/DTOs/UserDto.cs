namespace OptimalyChat.ServiceLayer.DTOs;

/// <summary>
/// DTO pro User entity s computed properties.
/// Dědí z BaseDto string pro správnou architekturu.
/// </summary>
public class UserDto : BaseDto<string>
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Computed properties - správně patří do DTO, ne do entity
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : UserName ?? Email ?? "Unknown User";
    
    // Time display properties
    public string CreatedAtDisplay => CreatedAt.Kind == DateTimeKind.Utc 
        ? CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm") 
        : CreatedAt.ToString("dd.MM.yyyy HH:mm");
}