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
/// Conversation service implementation
/// </summary>
public class ConversationService : BaseService<Conversation, ConversationDto, int>, IConversationService
{
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ConversationService> logger)
        : base(unitOfWork, mapper)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get conversations for a project
    /// </summary>
    public async Task<PagedResult<ConversationDto>> GetProjectConversationsAsync(int projectId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ConversationDto>>(items);

        // Populate display fields
        foreach (var dto in dtos)
        {
            var conversation = items.First(c => c.Id == dto.Id);
            dto.MessageCount = conversation.Messages.Count;
            dto.LastMessagePreview = conversation.Messages.FirstOrDefault()?.Content?.Substring(0, Math.Min(100, conversation.Messages.FirstOrDefault()?.Content?.Length ?? 0)) ?? "";
        }

        return new PagedResult<ConversationDto>
        {
            Items = dtos,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Create a new conversation in a project
    /// </summary>
    public async Task<ConversationDto> CreateConversationAsync(int projectId, string title, CancellationToken cancellationToken = default)
    {
        // Verify project exists
        var projectRepo = _unitOfWork.GetRepository<Project, int>();
        var projectExists = await projectRepo.Query.AnyAsync(p => p.Id == projectId, cancellationToken);
        
        if (!projectExists)
            throw new NotFoundException($"Project {projectId} not found");

        var conversation = new Conversation
        {
            ProjectId = projectId,
            Title = title
        };

        await _repository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ConversationDto>(conversation);
    }

    /// <summary>
    /// Get conversation with messages
    /// </summary>
    public async Task<ConversationDto> GetConversationWithMessagesAsync(int conversationId, int messageLimit = 50, CancellationToken cancellationToken = default)
    {
        var conversation = await _repository.Query
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(messageLimit))
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

        if (conversation == null)
            throw new NotFoundException($"Conversation {conversationId} not found");

        var dto = _mapper.Map<ConversationDto>(conversation);
        
        // Reverse messages to chronological order
        dto.Messages = dto.Messages.OrderBy(m => m.CreatedAt).ToList();
        dto.MessageCount = conversation.Messages.Count;

        return dto;
    }

    /// <summary>
    /// Update conversation title
    /// </summary>
    public async Task<ConversationDto> UpdateTitleAsync(int conversationId, string newTitle, CancellationToken cancellationToken = default)
    {
        var conversation = await _repository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null)
            throw new NotFoundException($"Conversation {conversationId} not found");

        conversation.Title = newTitle;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ConversationDto>(conversation);
    }

    /// <summary>
    /// Delete conversation and all messages
    /// </summary>
    public async Task DeleteConversationAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _repository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null)
            throw new NotFoundException($"Conversation {conversationId} not found");

        // Delete will cascade to messages
        _repository.Delete(conversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Search conversations in a project
    /// </summary>
    public async Task<IEnumerable<ConversationDto>> SearchConversationsAsync(int projectId, string searchTerm, CancellationToken cancellationToken = default)
    {
        var conversations = await _repository.Query
            .Where(c => c.ProjectId == projectId)
            .Where(c => c.Title.Contains(searchTerm) || 
                       c.Messages.Any(m => m.Content != null && m.Content.Contains(searchTerm)))
            .Include(c => c.Messages.Where(m => m.Content != null && m.Content.Contains(searchTerm)).Take(5))
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ConversationDto>>(conversations);
    }
}