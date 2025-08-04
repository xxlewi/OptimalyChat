using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.PresentationLayer.ViewModels;

/// <summary>
/// Export view model following CRM pattern
/// </summary>
public class ExportViewModel : BaseViewModel
{
    /// <summary>
    /// Available export formats
    /// </summary>
    public IEnumerable<ExportFormatOption> AvailableFormats { get; set; } = new List<ExportFormatOption>
    {
        new ExportFormatOption { Value = ExportFormat.Csv, Name = "CSV", Description = "Comma-separated values", Icon = "fas fa-file-csv", Recommended = true },
        new ExportFormatOption { Value = ExportFormat.Excel, Name = "Excel", Description = "Microsoft Excel", Icon = "fas fa-file-excel" },
        new ExportFormatOption { Value = ExportFormat.Json, Name = "JSON", Description = "JavaScript Object Notation", Icon = "fas fa-file-code" },
        new ExportFormatOption { Value = ExportFormat.Pdf, Name = "PDF", Description = "Portable Document Format", Icon = "fas fa-file-pdf" }
    };

    /// <summary>
    /// Export statistics (loaded via AJAX)
    /// </summary>
    public ExportStats? Stats { get; set; }

    /// <summary>
    /// Error message if export failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Success message if export completed
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Whether there was an error
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Whether there was a success message
    /// </summary>
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);
}

/// <summary>
/// Export format option for UI
/// </summary>
public class ExportFormatOption
{
    public ExportFormat Value { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool Recommended { get; set; }
}

/// <summary>
/// Export statistics for dashboard
/// </summary>
public class ExportStats
{
    public EntityStats Products { get; set; } = new();
    public EntityStats Categories { get; set; } = new();
    public EntityStats Users { get; set; } = new();
}

/// <summary>
/// Statistics for single entity type
/// </summary>
public class EntityStats
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    
    public double ActivePercentage => Total > 0 ? Math.Round((double)Active / Total * 100, 1) : 0;
    public double InactivePercentage => Total > 0 ? Math.Round((double)Inactive / Total * 100, 1) : 0;
}