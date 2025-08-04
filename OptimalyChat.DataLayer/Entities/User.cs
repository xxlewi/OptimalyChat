using Microsoft.AspNetCore.Identity;
using OptimalyChat.DataLayer.Interfaces;

namespace OptimalyChat.DataLayer.Entities;

public class User : IdentityUser, IBaseEntity<string>
{
    // IBaseEntity implementation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Vlastní properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}