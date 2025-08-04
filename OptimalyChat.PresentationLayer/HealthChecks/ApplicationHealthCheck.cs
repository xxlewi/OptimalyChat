using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace OptimalyChat.PresentationLayer.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApplicationHealthCheck> _logger;

    public ApplicationHealthCheck(IWebHostEnvironment environment, ILogger<ApplicationHealthCheck> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";
            var buildDate = GetBuildDate(assembly);

            var data = new Dictionary<string, object>
            {
                { "version", version },
                { "environment", _environment.EnvironmentName },
                { "buildDate", buildDate },
                { "machineName", Environment.MachineName },
                { "processId", Environment.ProcessId },
                { "workingSet", GC.GetTotalMemory(false) },
                { "uptime", DateTime.UtcNow.Subtract(System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds }
            };

            _logger.LogDebug("Application health check proběhl úspěšně");

            return Task.FromResult(HealthCheckResult.Healthy(
                "Aplikace běží správně",
                data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při kontrole aplikace");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Aplikace má problémy",
                ex));
        }
    }

    private static DateTime GetBuildDate(Assembly assembly)
    {
        const string buildVersionMetadataPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(buildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value.Substring(index + buildVersionMetadataPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var result))
                    return result;
            }
        }

        return new FileInfo(assembly.Location).CreationTime;
    }
}