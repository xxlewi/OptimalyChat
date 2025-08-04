using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace OptimalyChat.DataLayer.Interfaces;

/// <summary>
/// Generický repository interface s podporou různých typů ID
/// </summary>
/// <typeparam name="TEntity">Typ entity</typeparam>
/// <typeparam name="TKey">Typ primárního klíče</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class, IBaseEntity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);
    
    /// <summary>
    /// Provides access to IQueryable for complex queries
    /// </summary>
    IQueryable<TEntity> Query { get; }
}

/// <summary>
/// Repository interface pro entity s int ID (backward compatibility)
/// </summary>
/// <typeparam name="TEntity">Typ entity</typeparam>
public interface IRepository<TEntity> : IRepository<TEntity, int> where TEntity : class, IBaseEntity<int>
{
}