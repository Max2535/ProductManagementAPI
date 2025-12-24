namespace ProductManagement.Application.Common;

/// <summary>
/// Result Pattern for handling success/failure scenarios
/// Best Practice: แทนที่การ throw exceptions สำหรับ business logic errors
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, string message, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? new List<string>();
    }

    public static Result Success(string message = "Operation completed successfully")
        => new(true, message);

    public static Result Failure(string message, List<string>? errors = null)
        => new(false, message, errors);

    public static Result Failure(string message, string error)
        => new(false, message, new List<string> { error });
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, string message, T? data = default, List<string>? errors = null)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string message = "Operation completed successfully")
        => new(true, message, data);

    public new static Result<T> Failure(string message, List<string>? errors = null)
        => new(false, message, default, errors);

    public new static Result<T> Failure(string message, string error)
        => new(false, message, default, new List<string> { error });
}