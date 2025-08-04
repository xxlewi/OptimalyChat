namespace OptimalyChat.ServiceLayer.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message, "NOT_FOUND")
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"Entity '{entityName}' with key '{key}' was not found.", "NOT_FOUND")
    {
    }

    public NotFoundException(Type entityType, object key)
        : base($"Entity of type '{entityType.Name}' with key '{key}' was not found.", "NOT_FOUND")
    {
    }
}