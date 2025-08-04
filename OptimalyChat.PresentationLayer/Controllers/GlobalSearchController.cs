using Microsoft.AspNetCore.Mvc;
using OptimalyChat.ServiceLayer.Interfaces;
using OptimalyChat.PresentationLayer.ViewModels;

namespace OptimalyChat.PresentationLayer.Controllers;

/// <summary>
/// Global search controller following CRM pattern
/// Provides unified search functionality across all entities
/// </summary>
public class GlobalSearchController : Controller
{
    private readonly ISearchService _searchService;
    private readonly ILogger<GlobalSearchController> _logger;

    public GlobalSearchController(ISearchService searchService, ILogger<GlobalSearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Global search page
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View(new GlobalSearchViewModel());
    }

    /// <summary>
    /// Perform global search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string q, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return View("Index", new GlobalSearchViewModel());
        }

        try
        {
            var result = await _searchService.GlobalSearchAsync(q.Trim(), cancellationToken);
            
            var viewModel = new GlobalSearchViewModel
            {
                Query = q.Trim(),
                SearchResult = result,
                HasSearched = true
            };

            // Log search for analytics
            _logger.LogInformation("Global search performed: Query='{Query}', Results={ResultCount}, ExecutionTime={ExecutionTime}ms", 
                q, result.TotalResults, result.ExecutionTimeMs);

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing global search for query '{Query}'", q);
            
            var errorModel = new GlobalSearchViewModel
            {
                Query = q.Trim(),
                HasSearched = true,
                ErrorMessage = "Při vyhledávání došlo k chybě. Zkuste to prosím znovu."
            };
            
            return View("Index", errorModel);
        }
    }

    /// <summary>
    /// AJAX endpoint for search suggestions/autocomplete
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Suggestions(string term, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
        {
            return Json(Array.Empty<string>());
        }

        try
        {
            var suggestions = await _searchService.GetSearchSuggestionsAsync(term.Trim(), maxResults, cancellationToken);
            return Json(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions for term '{Term}'", term);
            return Json(Array.Empty<string>());
        }
    }

    /// <summary>
    /// AJAX endpoint for quick search (returns JSON)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> QuickSearch(string q, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Json(new { success = false, message = "Search query is required" });
        }

        try
        {
            var result = await _searchService.GlobalSearchAsync(q.Trim(), cancellationToken);
            
            return Json(new 
            { 
                success = true,
                query = q.Trim(),
                totalResults = result.TotalResults,
                executionTime = result.ExecutionTimeMs,
                resultCounts = result.ResultCounts,
                users = result.Users.Take(5).Select(u => new 
                {
                    id = u.Id,
                    name = u.FullName,
                    email = u.Email
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing quick search for query '{Query}'", q);
            return Json(new { success = false, message = "Při vyhledávání došlo k chybě" });
        }
    }
}