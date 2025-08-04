using System.Reflection;
using System.Text;
using System.Text.Json;
using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.ServiceLayer.Services;

/// <summary>
/// Export service implementation following CRM pattern
/// Provides data export functionality in various formats
/// Note: This is a basic implementation. For production, consider using libraries like:
/// - ClosedXML or EPPlus for Excel
/// - iTextSharp for PDF
/// - CsvHelper for CSV
/// </summary>
public class ExportService : IExportService
{
    /// <summary>
    /// Export data to Excel format (basic CSV implementation)
    /// In production, use ClosedXML or EPPlus for proper Excel files
    /// </summary>
    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Data", CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        // For now, return CSV format as Excel alternative
        // In production, implement proper Excel generation
        return await ExportToCsvAsync(data, true, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Export data to CSV format
    /// </summary>
    public async Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, bool includeHeaders = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        var csv = new StringBuilder();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
            .ToArray();

        // Add headers
        if (includeHeaders)
        {
            var headers = properties.Select(p => EscapeCsvField(p.Name));
            csv.AppendLine(string.Join(",", headers));
        }

        // Add data rows
        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvField(value?.ToString() ?? string.Empty);
            });
            
            csv.AppendLine(string.Join(",", values));
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString())).ConfigureAwait(false);
    }

    /// <summary>
    /// Export data to PDF format (basic text implementation)
    /// In production, use iTextSharp or similar for proper PDF generation
    /// </summary>
    public async Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string title = "Export", CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        var content = new StringBuilder();
        content.AppendLine($"# {title}");
        content.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        content.AppendLine();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
            .ToArray();

        // Add table header
        var headers = properties.Select(p => p.Name.PadRight(20));
        content.AppendLine(string.Join(" | ", headers));
        content.AppendLine(new string('-', headers.Sum(h => h.Length) + (headers.Count() - 1) * 3));

        // Add data rows
        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return (value?.ToString() ?? string.Empty).PadRight(20);
            });
            
            content.AppendLine(string.Join(" | ", values));
        }

        // In production, convert this to actual PDF
        return await Task.FromResult(Encoding.UTF8.GetBytes(content.ToString())).ConfigureAwait(false);
    }

    /// <summary>
    /// Export data to JSON format
    /// </summary>
    public async Task<byte[]> ExportToJsonAsync<T>(IEnumerable<T> data, bool formatted = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = formatted,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(data, options);
        return await Task.FromResult(Encoding.UTF8.GetBytes(json)).ConfigureAwait(false);
    }

    /// <summary>
    /// Check if type is simple (can be exported easily)
    /// </summary>
    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(DateTime) ||
               type == typeof(DateTime?) ||
               type == typeof(decimal) ||
               type == typeof(decimal?) ||
               type == typeof(Guid) ||
               type == typeof(Guid?);
    }

    /// <summary>
    /// Escape CSV field for proper formatting
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "\"\"";

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}