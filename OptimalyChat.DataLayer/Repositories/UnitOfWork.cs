using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OptimalyChat.DataLayer.Data;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;

namespace OptimalyChat.DataLayer.Repositories;

/// <summary>
/// Implementace Unit of Work pattern s repository management
/// Poskytuje centralizovaný přístup k repositories a transaction management
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    
    // Repository cache - lazy loading
    private readonly Dictionary<Type, object> _repositories = new();
    private IUserRepository? _users;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Repository properties - lazy loading
    public IUserRepository Users => _users ??= new UserRepository(_context);

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new Repository<TEntity>(_context);
            _repositories[type] = repository;
        }
        return (IRepository<TEntity>)repository;
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class, IBaseEntity<TKey>
    {
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new BaseRepository<TEntity, TKey>(_context);
            _repositories[type] = repository;
        }
        return (IRepository<TEntity, TKey>)repository;
    }

    // Transaction management
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit logic - správně patří sem do UnitOfWork, ne do DbContext
        ApplyAuditInformation();
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private void ApplyAuditInformation()
    {
        var entries = _context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IBaseEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (IBaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
                // Zajistíme, že CreatedAt se nezmění
                entry.Property(nameof(IBaseEntity.CreatedAt)).IsModified = false;
            }
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (_currentTransaction != null)
                await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync().ConfigureAwait(false);
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
                await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync().ConfigureAwait(false);
                _currentTransaction = null;
            }
        }
    }

    // Bulk operations
    public async Task<int> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default, params object[] parameters)
    {
        return await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken).ConfigureAwait(false);
    }

    // IDisposable implementation
    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    // IAsyncDisposable implementation
    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
            await _currentTransaction.DisposeAsync();
        
        await _context.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}