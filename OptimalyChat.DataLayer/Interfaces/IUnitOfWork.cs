using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Interfaces;

/// <summary>
/// Unit of Work pattern interface s repository management
/// Centralizuje práci s více repositories v jedné transakci
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Repository properties - lazy loading
    IUserRepository Users { get; }
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;
    IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class, IBaseEntity<TKey>;
    
    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<int> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default, params object[] parameters);
}