using Microsoft.EntityFrameworkCore;
using OptimalyChat.DataLayer.Data;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;

namespace OptimalyChat.DataLayer.Repositories;

/// <summary>
/// Repository implementace pro User entity s string ID
/// Dědí z BaseRepository a přidává User-specifické metody
/// </summary>
public class UserRepository : BaseRepository<User, string>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken).ConfigureAwait(false);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        // IsDeleted filter je aplikován automaticky přes query filter
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetActiveUsersAsync(cancellationToken).ConfigureAwait(false);
        
        // IsDeleted filter je aplikován automaticky přes query filter
        return await _context.Users
            .Where(u => u.IsActive)
            .Where(u => 
                u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (u.Email != null && u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (u.UserName != null && u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    // Poznámka: Základní CRUD operace jsou zděděny z BaseRepository<User, string>

    public async Task UpdateLastLoginAsync(string emailOrUserId, CancellationToken cancellationToken = default)
    {
        // Pokus se najít podle emailu, pak podle ID
        var user = await GetByEmailAsync(emailOrUserId, cancellationToken).ConfigureAwait(false) 
                   ?? await GetByIdAsync(emailOrUserId, cancellationToken).ConfigureAwait(false);
        
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            Update(user);
        }
    }
}