using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OptimalyChat.PresentationLayer.Controllers;

[Authorize]
public class HealthController : Controller
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Zobrazení health check stránky");
        
        var report = await _healthCheckService.CheckHealthAsync();
        
        var viewModel = new HealthViewModel
        {
            Status = report.Status,
            TotalDuration = report.TotalDuration,
            Checks = report.Entries.Select(kvp => new HealthCheckViewModel
            {
                Name = kvp.Key,
                Status = kvp.Value.Status,
                Description = kvp.Value.Description ?? "Bez popisu",
                Duration = kvp.Value.Duration,
                Exception = kvp.Value.Exception?.Message,
                Data = kvp.Value.Data
            }).ToList()
        };

        return View(viewModel);
    }
}

public class HealthViewModel
{
    public HealthStatus Status { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<HealthCheckViewModel> Checks { get; set; } = new();
}

public class HealthCheckViewModel
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? Exception { get; set; }
    public IReadOnlyDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
}