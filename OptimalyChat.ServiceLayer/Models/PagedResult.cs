namespace OptimalyChat.ServiceLayer.Models;

/// <summary>
/// Generic paged result container
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Items in current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    
    /// <summary>
    /// Has previous page
    /// </summary>
    public bool HasPrevious => Page > 1;
    
    /// <summary>
    /// Has next page
    /// </summary>
    public bool HasNext => Page < TotalPages;
    
    /// <summary>
    /// Items on current page
    /// </summary>
    public int ItemsCount => Items.Count();
    
    /// <summary>
    /// Start item number (1-based)
    /// </summary>
    public int StartItem => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
    
    /// <summary>
    /// End item number (1-based)
    /// </summary>
    public int EndItem => Math.Min(StartItem + ItemsCount - 1, TotalItems);
    
    /// <summary>
    /// Create paged result
    /// </summary>
    public static PagedResult<T> Create(IEnumerable<T> items, int page, int pageSize, int totalItems)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}