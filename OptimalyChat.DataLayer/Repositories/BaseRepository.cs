using Microsoft.EntityFrameworkCore;
using OptimalyChat.DataLayer.Data;
using OptimalyChat.DataLayer.Interfaces;
using System.Linq.Expressions;

namespace OptimalyChat.DataLayer.Repositories;

/// <summary>
/// Základní generic repository implementace pro jakýkoliv typ entity a ID
/// Skutečně generic - pracuje s TEntity, TKey
/// </summary>
/// <typeparam name="TEntity">Typ entity implementující IBaseEntity TKey</typeparam>
/// <typeparam name="TKey">Typ primárního klíče</typeparam>
public class BaseRepository<TEntity, TKey> : IRepository<TEntity, TKey> 
    where TEntity : class, IBaseEntity<TKey>
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public virtual IQueryable<TEntity> Query => _dbSet;

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        // Soft delete - pouze nastavíme IsDeleted = true
        entity.IsDeleted = true;
        _dbSet.Update(entity);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
        }
        _dbSet.UpdateRange(entities);
    }
}