using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OptimalyChat.DataLayer.Interfaces;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Exceptions;
using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.ServiceLayer.Services;

/// <summary>
/// Generický base service s podporou různých typů ID
/// </summary>
/// <typeparam name="TEntity">Entity typ</typeparam>
/// <typeparam name="TDto">DTO typ</typeparam>
/// <typeparam name="TKey">Typ primárního klíče</typeparam>
public abstract class BaseService<TEntity, TDto, TKey> : IBaseService<TDto, TKey>
    where TEntity : class, IBaseEntity<TKey>
    where TDto : BaseDto<TKey>
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    protected readonly IRepository<TEntity, TKey> _repository;

    protected BaseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _repository = _unitOfWork.GetRepository<TEntity, TKey>();
    }

    public virtual async Task<TDto?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        var entity = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity == null ? null : _mapper.Map<TDto>(entity);
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public virtual async Task<TDto> CreateAsync(TDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        try
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<TDto>(entity);
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException($"Failed to create {typeof(TEntity).Name}", ex, "CREATE_FAILED");
        }
    }

    public virtual async Task<TDto> UpdateAsync(TDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        try
        {
            // Načteme existující entitu z databáze
            var existingEntity = await _repository.GetByIdAsync(((IBaseDto<TKey>)dto).Id, cancellationToken).ConfigureAwait(false);
            if (existingEntity == null)
            {
                throw new NotFoundException(typeof(TEntity).Name, ((IBaseDto<TKey>)dto).Id);
            }
            
            // Mapujeme pouze změněné hodnoty
            _mapper.Map(dto, existingEntity);
            _repository.Update(existingEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<TDto>(existingEntity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new BusinessException($"Entity {typeof(TEntity).Name} was modified by another user", ex, "CONCURRENCY_CONFLICT");
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException($"Failed to update {typeof(TEntity).Name}", ex, "UPDATE_FAILED");
        }
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        var entity = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity == null)
        {
            throw new NotFoundException(typeof(TEntity).Name, id);
        }
        
        try
        {
            _repository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException($"Failed to delete {typeof(TEntity).Name}", ex, "DELETE_FAILED");
        }
    }
}

/// <summary>
/// Base service pro entity s int ID (backward compatibility)
/// </summary>
/// <typeparam name="TEntity">Entity typ</typeparam>
/// <typeparam name="TDto">DTO typ</typeparam>
public abstract class BaseService<TEntity, TDto> : BaseService<TEntity, TDto, int>
    where TEntity : class, IBaseEntity<int>
    where TDto : BaseDto
{
    protected BaseService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }
}