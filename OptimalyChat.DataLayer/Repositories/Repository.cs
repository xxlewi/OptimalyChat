using OptimalyChat.DataLayer.Data;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;

namespace OptimalyChat.DataLayer.Repositories;

/// <summary>
/// Repository implementace pro entity s int ID (dědí z BaseEntity)
/// Poskytuje backward compatibility s původním IRepository TEntity interface
/// </summary>
/// <typeparam name="TEntity">Typ entity dědící z BaseEntity (int ID)</typeparam>
public class Repository<TEntity> : BaseRepository<TEntity, int>, IRepository<TEntity> 
    where TEntity : BaseEntity
{
    public Repository(ApplicationDbContext context) : base(context)
    {
    }
}