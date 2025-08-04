namespace OptimalyChat.DataLayer.Interfaces;

/// <summary>
/// Základní interface pro entity s audit poli a soft delete
/// </summary>
public interface IBaseEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    bool IsDeleted { get; set; }
}

/// <summary>
/// Rozšířený interface pro entity s typovaným ID
/// </summary>
/// <typeparam name="TKey">Typ primárního klíče</typeparam>
public interface IBaseEntity<TKey> : IBaseEntity
{
    TKey Id { get; set; }
}