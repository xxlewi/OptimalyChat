using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Models;

namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// Project service interface for managing AI chat projects
/// </summary>
public interface IProjectService : IBaseService<ProjectDto>
{
    /// <summary>
    /// Get projects for a specific user
    /// </summary>
    Task<PagedResult<ProjectDto>> GetUserProjectsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a new project for a user
    /// </summary>
    Task<ProjectDto> CreateProjectAsync(string userId, ProjectDto projectDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get project with conversations
    /// </summary>
    Task<ProjectDto> GetProjectWithConversationsAsync(int projectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update project encryption settings
    /// </summary>
    Task<ProjectDto> UpdateEncryptionAsync(int projectId, bool isEncrypted, string encryptionLevel, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete project and all related data
    /// </summary>
    Task DeleteProjectAsync(int projectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get project statistics
    /// </summary>
    Task<ProjectStatisticsDto> GetProjectStatisticsAsync(int projectId, CancellationToken cancellationToken = default);
}