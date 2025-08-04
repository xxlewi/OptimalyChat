using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// Service interface pro User management
/// Extends base service s User-specific operacemi
/// </summary>
public interface IUserService : IBaseService<UserDto, string>
{
    /// <summary>
    /// Najde uživatele podle emailu
    /// </summary>
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá všechny aktivní uživatele
    /// </summary>
    Task<IEnumerable<UserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Vyhledá uživatele podle search termu (jméno, příjmení, email, username)
    /// </summary>
    Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aktualizuje poslední přihlášení uživatele
    /// </summary>
    Task UpdateLastLoginAsync(string emailOrUserId, CancellationToken cancellationToken = default);
}