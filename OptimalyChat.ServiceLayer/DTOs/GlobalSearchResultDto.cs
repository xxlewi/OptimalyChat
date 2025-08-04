namespace OptimalyChat.ServiceLayer.DTOs;

/// <summary>
/// Global search result DTO following CRM pattern
/// Contains search results grouped by entity type
/// </summary>
public class GlobalSearchResultDto
{
    /// <summary>
    /// Search query that was executed
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of results across all entity types
    /// </summary>
    public int TotalResults { get; set; }
    
    /// <summary>
    /// Search execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    /// <summary>
    /// User search results
    /// </summary>
    public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
    
    /// <summary>
    /// Search result summary by entity type
    /// </summary>
    public Dictionary<string, int> ResultCounts { get; set; } = new Dictionary<string, int>();
    
    /// <summary>
    /// Search suggestions for query improvement
    /// </summary>
    public IEnumerable<string> Suggestions { get; set; } = new List<string>();
    
    /// <summary>
    /// Whether the search had any results
    /// </summary>
    public bool HasResults => TotalResults > 0;
    
    /// <summary>
    /// Formatted summary of search results
    /// </summary>
    public string ResultSummary => TotalResults switch
    {
        0 => $"Žádné výsledky pro '{Query}'",
        1 => $"1 výsledek pro '{Query}'",
        _ => $"{TotalResults} výsledků pro '{Query}'"
    };
}