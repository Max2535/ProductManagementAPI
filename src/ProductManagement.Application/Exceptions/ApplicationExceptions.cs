namespace ProductManagement.Application.Exceptions;

/// <summary>
/// Custom exception classes for better error handling
/// Best Practice: แยก exception types ตาม business scenarios
/// </summary>

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entity, object key)
        : base($"{entity} with key '{key}' was not found")
    {
    }
}

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(List<string> errors)
        : base("One or more validation errors occurred")
    {
        Errors = errors;
    }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

public class DuplicateException : Exception
{
    public DuplicateException(string message) : base(message)
    {
    }

    public DuplicateException(string entity, string field, object value)
        : base($"{entity} with {field} '{value}' already exists")
    {
    }
}