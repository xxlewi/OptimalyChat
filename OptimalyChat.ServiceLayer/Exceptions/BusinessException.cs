namespace OptimalyChat.ServiceLayer.Exceptions;

public class BusinessException : Exception
{
    public string Code { get; set; }
    public object? Details { get; set; }

    public BusinessException(string message, string code = "BUSINESS_ERROR") : base(message)
    {
        Code = code;
    }

    public BusinessException(string message, Exception innerException, string code = "BUSINESS_ERROR") 
        : base(message, innerException)
    {
        Code = code;
    }

    public BusinessException(string message, object details, string code = "BUSINESS_ERROR") : base(message)
    {
        Code = code;
        Details = details;
    }
}