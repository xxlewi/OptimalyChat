using System.Net;
using System.Text.Json;
using OptimalyChat.ServiceLayer.Exceptions;

namespace OptimalyChat.PresentationLayer.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Došlo k neočekávané chybě");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, new ErrorDetail
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = notFound.Message,
                ErrorCode = notFound.Code
            }),

            ValidationException validation => (HttpStatusCode.BadRequest, new ValidationErrorDetail
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = validation.Message,
                ErrorCode = validation.Code,
                Errors = validation.Errors
            }),

            BusinessException business => (HttpStatusCode.BadRequest, new ErrorDetail
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = business.Message,
                ErrorCode = business.Code
            }),

            _ => (HttpStatusCode.InternalServerError, _environment.IsDevelopment()
                ? new ErrorDetail
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = exception.Message,
                    ErrorCode = "INTERNAL_ERROR",
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                }
                : new ErrorDetail
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Došlo k interní chybě serveru. Kontaktujte prosím administrátora.",
                    ErrorCode = "INTERNAL_ERROR"
                })
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorDetail
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
}

public class ValidationErrorDetail : ErrorDetail
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
}