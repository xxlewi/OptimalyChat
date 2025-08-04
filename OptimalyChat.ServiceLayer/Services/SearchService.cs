using System.Diagnostics;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OptimalyChat.DataLayer.Interfaces;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.ServiceLayer.Services;

/// <summary>
/// Global search service implementation following CRM pattern
/// Provides unified search across multiple entity types with performance optimization
/// </summary>
public class SearchService : ISearchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Perform global search across all searchable entities
    /// </summary>
    public async Task<GlobalSearchResultDto> GlobalSearchAsync(string query, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        
        var stopwatch = Stopwatch.StartNew();
        var result = new GlobalSearchResultDto
        {
            Query = query,
            ResultCounts = new Dictionary<string, int>()
        };

        try
        {
            var searchTerm = query.Trim().ToLower();
            
            // Search users
            var userRepository = _unitOfWork.Users;
            var users = await userRepository.Query
                .Where(u => u.Email.ToLower().Contains(searchTerm) ||
                           (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                           (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)))
                .Take(10)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
                
            result.Users = _mapper.Map<IEnumerable<UserDto>>(users);
            result.ResultCounts["Users"] = users.Count;
            
            // TODO: Add search for Projects, Conversations, Messages when implemented
            
            // Calculate totals
            result.TotalResults = result.ResultCounts.Values.Sum();
            
            // Generate suggestions
            result.Suggestions = GenerateSearchSuggestions(query, result);
        }
        finally
        {
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    /// <summary>
    /// Search within a specific entity type
    /// </summary>
    public async Task<IEnumerable<TDto>> SearchAsync<TDto>(string query, CancellationToken cancellationToken = default) 
        where TDto : BaseDto
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        
        var searchTerm = query.Trim().ToLower();
        var entityType = typeof(TDto);
        
        if (entityType == typeof(UserDto))
        {
            var users = await SearchUsersAsync(searchTerm, 20, cancellationToken);
            return (IEnumerable<TDto>)(object)users;
        }
        
        // TODO: Add support for other entity types
        
        throw new NotSupportedException($"Search for type {entityType.Name} is not implemented");
    }

    /// <summary>
    /// Get search suggestions based on query
    /// </summary>
    public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string partialQuery, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partialQuery);
        
        var suggestions = new List<string>();
        var searchTerm = partialQuery.Trim().ToLower();
        
        // Get user email suggestions
        var userEmails = await _unitOfWork.Users.Query
            .Where(u => u.Email.ToLower().StartsWith(searchTerm))
            .Select(u => u.Email)
            .Take(maxResults / 2)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
            
        suggestions.AddRange(userEmails);
        
        // TODO: Add suggestions from other entities
        
        return suggestions.Distinct().Take(maxResults).OrderBy(s => s);
    }

    private async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm, int limit, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.Query
            .Where(u => u.Email.ToLower().Contains(searchTerm) ||
                       (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                       (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)))
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
            
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    private IEnumerable<string> GenerateSearchSuggestions(string query, GlobalSearchResultDto result)
    {
        var suggestions = new List<string>();
        
        if (result.TotalResults == 0)
        {
            suggestions.Add($"Zkuste hledat '{query.Substring(0, Math.Min(3, query.Length))}'");
            suggestions.Add("Zkontrolujte překlepy");
            suggestions.Add("Použijte méně specifické termíny");
        }
        else if (result.TotalResults > 50)
        {
            suggestions.Add("Zpřesněte hledání přidáním dalších slov");
            suggestions.Add("Použijte filtrování podle typu");
        }
        
        return suggestions;
    }
}