using Microsoft.AspNetCore.Mvc;
using OptimalyChat.ServiceLayer.Interfaces;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.PresentationLayer.ViewModels;

namespace OptimalyChat.PresentationLayer.Controllers;

/// <summary>
/// Export controller following CRM pattern
/// Provides data export functionality in various formats
/// </summary>
public class ExportController : Controller
{
    private readonly IExportService _exportService;
    private readonly IUserService _userService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(
        IExportService exportService,
        IUserService userService,
        ILogger<ExportController> logger)
    {
        _exportService = exportService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Display export options page
    /// </summary>
    public IActionResult Index()
    {
        var model = new ExportViewModel();
        
        return View(model);
    }

    /// <summary>
    /// Export users data
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportUsers(string format, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Exporting users in {Format} format", format);
            
            var users = await _userService.GetAllAsync(cancellationToken);
            
            byte[] fileContent;
            string contentType;
            string fileExtension;
            
            switch (format.ToUpper())
            {
                case "CSV":
                    fileContent = await _exportService.ExportToCsvAsync(users, true, cancellationToken);
                    contentType = "text/csv";
                    fileExtension = "csv";
                    break;
                case "EXCEL":
                    fileContent = await _exportService.ExportToExcelAsync(users, "Users", cancellationToken);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileExtension = "xlsx";
                    break;
                case "JSON":
                    fileContent = await _exportService.ExportToJsonAsync(users, true, cancellationToken);
                    contentType = "application/json";
                    fileExtension = "json";
                    break;
                case "PDF":
                    fileContent = await _exportService.ExportToPdfAsync(users, "Users Export", cancellationToken);
                    contentType = "application/pdf";
                    fileExtension = "pdf";
                    break;
                default:
                    throw new NotSupportedException($"Format {format} is not supported");
            }
            
            var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";
            
            return File(fileContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting users in {Format} format", format);
            TempData["Error"] = $"Error exporting users: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Get export preview for entity type
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Preview(string entityType, CancellationToken cancellationToken)
    {
        try
        {
            var previewData = entityType?.ToLower() switch
            {
                "users" => await GetUsersPreview(cancellationToken),
                _ => new { Error = "Unsupported entity type" }
            };
            
            return Json(previewData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview for {EntityType}", entityType);
            return BadRequest(new { error = ex.Message });
        }
    }

    private async Task<object> GetUsersPreview(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        var usersList = users.Take(5).ToList();
        
        return new
        {
            EntityType = "Users",
            TotalCount = users.Count(),
            PreviewCount = usersList.Count,
            Columns = new[] { "Id", "Email", "FirstName", "LastName", "CreatedAt" },
            Data = usersList.Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
        };
    }
}