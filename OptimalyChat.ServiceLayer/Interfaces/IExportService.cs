using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.ServiceLayer.Interfaces;

/// <summary>
/// Export service interface following CRM pattern
/// Provides data export functionality in various formats
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export data to Excel format
    /// </summary>
    /// <typeparam name="T">Data type to export</typeparam>
    /// <param name="data">Data collection to export</param>
    /// <param name="sheetName">Excel sheet name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Excel file as byte array</returns>
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Data", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Export data to CSV format
    /// </summary>
    /// <typeparam name="T">Data type to export</typeparam>
    /// <param name="data">Data collection to export</param>
    /// <param name="includeHeaders">Include column headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CSV file as byte array</returns>
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, bool includeHeaders = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Export data to PDF format
    /// </summary>
    /// <typeparam name="T">Data type to export</typeparam>
    /// <param name="data">Data collection to export</param>
    /// <param name="title">Document title</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string title = "Export", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Export data to JSON format
    /// </summary>
    /// <typeparam name="T">Data type to export</typeparam>
    /// <param name="data">Data collection to export</param>
    /// <param name="formatted">Format JSON for readability</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON file as byte array</returns>
    Task<byte[]> ExportToJsonAsync<T>(IEnumerable<T> data, bool formatted = true, CancellationToken cancellationToken = default);
}

/// <summary>
/// Export format enumeration
/// </summary>
public enum ExportFormat
{
    Excel,
    Csv, 
    Pdf,
    Json
}