using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// Global search service interface following CRM pattern
/// Provides unified search across multiple entity types
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Perform global search across all searchable entities
    /// </summary>
    /// <param name="query">Search query string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results grouped by entity type</returns>
    Task<GlobalSearchResultDto> GlobalSearchAsync(string query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search within specific entity type
    /// </summary>
    /// <typeparam name="TDto">DTO type to search</typeparam>
    /// <param name="query">Search query string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching entities of specified type</returns>
    Task<IEnumerable<TDto>> SearchAsync<TDto>(string query, CancellationToken cancellationToken = default) 
        where TDto : BaseDto;
    
    /// <summary>
    /// Get search suggestions/auto-complete
    /// </summary>
    /// <param name="partialQuery">Partial search query</param>
    /// <param name="maxResults">Maximum number of suggestions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search suggestions</returns>
    Task<IEnumerable<string>> GetSearchSuggestionsAsync(string partialQuery, int maxResults = 10, CancellationToken cancellationToken = default);
}