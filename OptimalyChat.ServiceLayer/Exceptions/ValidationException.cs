namespace OptimalyChat.ServiceLayer.Exceptions;

public class ValidationException : BusinessException
{
    public Dictionary<string, string[]> Errors { get; set; }

    public ValidationException(string message) : base(message, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors) 
        : base("Validation failed", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base("Validation failed", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}