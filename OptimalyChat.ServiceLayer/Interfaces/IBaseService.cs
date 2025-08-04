using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// Generický base service interface s podporou různých typů ID
/// </summary>
/// <typeparam name="TDto">DTO typ</typeparam>
/// <typeparam name="TKey">Typ primárního klíče</typeparam>
public interface IBaseService<TDto, TKey> where TDto : BaseDto<TKey>
{
    Task<TDto?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TDto> CreateAsync(TDto dto, CancellationToken cancellationToken = default);
    Task<TDto> UpdateAsync(TDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base service interface pro entity s int ID (backward compatibility)
/// </summary>
/// <typeparam name="TDto">DTO typ</typeparam>
public interface IBaseService<TDto> : IBaseService<TDto, int> where TDto : BaseDto
{
}