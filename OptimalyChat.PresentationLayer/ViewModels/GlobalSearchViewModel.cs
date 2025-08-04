using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.PresentationLayer.ViewModels;

/// <summary>
/// Global search view model following CRM pattern
/// </summary>
public class GlobalSearchViewModel : BaseViewModel
{
    /// <summary>
    /// Search query entered by user
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Search results from service
    /// </summary>
    public GlobalSearchResultDto? SearchResult { get; set; }
    
    /// <summary>
    /// Whether a search has been performed
    /// </summary>
    public bool HasSearched { get; set; }
    
    /// <summary>
    /// Error message if search failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Whether there was an error during search
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    
    /// <summary>
    /// Whether search returned any results
    /// </summary>
    public bool HasResults => SearchResult?.HasResults == true;
    
    /// <summary>
    /// Total number of search results
    /// </summary>
    public int TotalResults => SearchResult?.TotalResults ?? 0;
    
    /// <summary>
    /// Search execution time for display
    /// </summary>
    public string ExecutionTime => SearchResult != null ? $"{SearchResult.ExecutionTimeMs} ms" : string.Empty;
    
    /// <summary>
    /// Result summary text for display
    /// </summary>
    public string ResultSummary => SearchResult?.ResultSummary ?? string.Empty;
    
    /// <summary>
    /// Whether to show search suggestions
    /// </summary>
    public bool ShowSuggestions => SearchResult?.Suggestions.Any() == true;
    
    /// <summary>
    /// Search suggestions for improving query
    /// </summary>
    public IEnumerable<string> Suggestions => SearchResult?.Suggestions ?? Array.Empty<string>();
    
    /// <summary>
    /// User search results
    /// </summary>
    public IEnumerable<UserDto> Users => SearchResult?.Users ?? Array.Empty<UserDto>();
    
    /// <summary>
    /// Result counts by entity type
    /// </summary>
    public Dictionary<string, int> ResultCounts => SearchResult?.ResultCounts ?? new Dictionary<string, int>();
    
    /// <summary>
    /// Whether products tab should be active (has most results)
    /// </summary>
    public bool ProductsTabActive => ResultCounts.TryGetValue("Products", out var productCount) && 
                                    productCount >= Math.Max(ResultCounts.GetValueOrDefault("Categories", 0), ResultCounts.GetValueOrDefault("Users", 0));
                                    
    /// <summary>
    /// Whether categories tab should be active
    /// </summary>
    public bool CategoriesTabActive => !ProductsTabActive && 
                                      ResultCounts.TryGetValue("Categories", out var categoryCount) && 
                                      categoryCount >= ResultCounts.GetValueOrDefault("Users", 0);
                                      
    /// <summary>
    /// Whether users tab should be active
    /// </summary>
    public bool UsersTabActive => !ProductsTabActive && !CategoriesTabActive;
    
    /// <summary>
    /// Get badge class for result count
    /// </summary>
    public string GetResultBadgeClass(string entityType)
    {
        var count = ResultCounts.GetValueOrDefault(entityType, 0);
        return count switch
        {
            0 => "badge-secondary",
            >= 10 => "badge-success",
            >= 5 => "badge-info",
            _ => "badge-primary"
        };
    }
    
    /// <summary>
    /// Get display text for result count
    /// </summary>
    public string GetResultCountText(string entityType)
    {
        var count = ResultCounts.GetValueOrDefault(entityType, 0);
        return count == 0 ? "Žádné" : count.ToString();
    }
}