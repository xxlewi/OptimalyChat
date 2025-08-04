using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Exceptions;
using OptimalyChat.ServiceLayer.Interfaces;
using OptimalyChat.ServiceLayer.Models;

namespace OptimalyChat.ServiceLayer.Services;

/// <summary>
/// Project service implementation
/// </summary>
public class ProjectService : BaseService<Project, ProjectDto, int>, IProjectService
{
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProjectService> logger)
        : base(unitOfWork, mapper)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get projects for a specific user
    /// </summary>
    public async Task<PagedResult<ProjectDto>> GetUserProjectsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ProjectDto>>(items);

        return new PagedResult<ProjectDto>
        {
            Items = dtos,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Create a new project for a user
    /// </summary>
    public async Task<ProjectDto> CreateProjectAsync(string userId, ProjectDto projectDto, CancellationToken cancellationToken = default)
    {
        // Check if user exists
        var userRepo = _unitOfWork.GetRepository<User, string>();
        var userExists = await userRepo.Query.AnyAsync(u => u.Id == userId, cancellationToken);
        
        if (!userExists)
            throw new NotFoundException($"User {userId} not found");

        var project = _mapper.Map<Project>(projectDto);
        project.UserId = userId;

        await _repository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProjectDto>(project);
    }

    /// <summary>
    /// Get project with conversations
    /// </summary>
    public async Task<ProjectDto> GetProjectWithConversationsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _repository.Query
            .Include(p => p.Conversations.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt))
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
            throw new NotFoundException($"Project {projectId} not found");

        return _mapper.Map<ProjectDto>(project);
    }

    /// <summary>
    /// Update project encryption settings
    /// </summary>
    public async Task<ProjectDto> UpdateEncryptionAsync(int projectId, bool isEncrypted, string encryptionLevel, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
            throw new NotFoundException($"Project {projectId} not found");

        // Validate encryption level
        if (!Enum.TryParse<EncryptionLevel>(encryptionLevel, out var level))
            throw new ValidationException($"Invalid encryption level: {encryptionLevel}");

        project.IsEncrypted = isEncrypted;
        project.EncryptionLevel = level;

        if (!isEncrypted)
        {
            project.EncryptionKeyId = null;
        }
        else if (string.IsNullOrEmpty(project.EncryptionKeyId))
        {
            // Generate new encryption key ID
            project.EncryptionKeyId = Guid.NewGuid().ToString();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ProjectDto>(project);
    }

    /// <summary>
    /// Delete project and all related data
    /// </summary>
    public async Task DeleteProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
            throw new NotFoundException($"Project {projectId} not found");

        // Delete will cascade to conversations, messages, and documents
        _repository.Delete(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    public async Task<ProjectStatisticsDto> GetProjectStatisticsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _repository.Query
            .Include(p => p.Conversations)
                .ThenInclude(c => c.Messages)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
            throw new NotFoundException($"Project {projectId} not found");

        var allMessages = project.Conversations.SelectMany(c => c.Messages).ToList();

        var stats = new ProjectStatisticsDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            TotalConversations = project.Conversations.Count,
            TotalMessages = allMessages.Count,
            TotalTokensUsed = project.Conversations.Sum(c => c.TotalTokensUsed),
            TotalDocuments = project.Documents.Count,
            TotalStorageBytes = project.Documents.Sum(d => d.FileSize),
            LastActivityAt = project.Conversations
                .Where(c => c.LastMessageAt.HasValue)
                .Select(c => c.LastMessageAt!.Value)
                .OrderByDescending(d => d)
                .FirstOrDefault(),
            CreatedAt = project.CreatedAt,
            MessagesByRole = allMessages
                .GroupBy(m => m.Role)
                .ToDictionary(g => g.Key, g => g.Count()),
            TokensByModel = allMessages
                .Where(m => !string.IsNullOrEmpty(m.Model))
                .GroupBy(m => m.Model!)
                .ToDictionary(g => g.Key, g => g.Sum(m => m.TokenCount)),
            AverageResponseTimeMs = allMessages
                .Where(m => m.ResponseTimeMs.HasValue)
                .Select(m => m.ResponseTimeMs!.Value)
                .DefaultIfEmpty(0)
                .Average()
        };

        return stats;
    }
}