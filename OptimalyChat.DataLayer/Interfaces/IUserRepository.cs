using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Interfaces;

/// <summary>
/// Repository interface pro User entity (string ID)
/// Rozšiřuje základní repository pattern o User-specifické metody
/// </summary>
public interface IUserRepository : IRepository<User, string>
{
    // User-specifické metody
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(string emailOrUserId, CancellationToken cancellationToken = default);
}