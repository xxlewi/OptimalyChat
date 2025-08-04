using AutoMapper;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Exceptions;
using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.ServiceLayer.Services;

/// <summary>
/// User service implementace
/// Dědí z BaseService a přidává User-specific business logiku
/// </summary>
public class UserService : BaseService<User, UserDto, string>, IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
        _userRepository = unitOfWork.Users;
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException(nameof(email), "Email cannot be empty");
        
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetActiveUsersAsync(cancellationToken).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ValidationException(nameof(searchTerm), "Search term cannot be empty");
        
        var users = await _userRepository.SearchUsersAsync(searchTerm, cancellationToken).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task UpdateLastLoginAsync(string emailOrUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(emailOrUserId))
            throw new ValidationException(nameof(emailOrUserId), "Email or User ID cannot be empty");
        
        await _userRepository.UpdateLastLoginAsync(emailOrUserId, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    // Override Create pro User-specific validace
    public override async Task<UserDto> CreateAsync(UserDto dto, CancellationToken cancellationToken = default)
    {
        // User-specific business validations
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException(nameof(dto.Email), "Email is required");
        
        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new ValidationException(nameof(dto.UserName), "Username is required");
        
        // Check for email uniqueness
        var existingUser = await GetByEmailAsync(dto.Email, cancellationToken).ConfigureAwait(false);
        if (existingUser != null)
            throw new BusinessException($"User with email '{dto.Email}' already exists", "USER_EXISTS");
        
        return await base.CreateAsync(dto, cancellationToken).ConfigureAwait(false);
    }

    // Override Update pro User-specific validace  
    public override async Task<UserDto> UpdateAsync(UserDto dto, CancellationToken cancellationToken = default)
    {
        // User-specific business validations
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException(nameof(dto.Email), "Email is required");
        
        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new ValidationException(nameof(dto.UserName), "Username is required");
        
        // Check if user exists
        var existingUser = await GetByIdAsync(dto.Id, cancellationToken).ConfigureAwait(false);
        if (existingUser == null)
            throw new NotFoundException(nameof(User), dto.Id);
        
        // Check for email uniqueness (exclude current user)
        var userWithEmail = await GetByEmailAsync(dto.Email, cancellationToken).ConfigureAwait(false);
        if (userWithEmail != null && userWithEmail.Id != dto.Id)
            throw new BusinessException($"Another user with email '{dto.Email}' already exists", "EMAIL_EXISTS");
        
        return await base.UpdateAsync(dto, cancellationToken).ConfigureAwait(false);
    }
}