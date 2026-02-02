namespace Clientes.Services;

public enum OperationErrorType
{
    None,
    InvalidInput,
    Conflict,
    NotFound,
    Unauthorized,
    Unexpected
}

public record OperationResult(bool Succeeded, OperationErrorType ErrorType = OperationErrorType.None, string? ErrorMessage = null)
{
    public static OperationResult Success() => new(true);
    public static OperationResult Invalid(string message) => new(false, OperationErrorType.InvalidInput, message);
    public static OperationResult Conflict(string message) => new(false, OperationErrorType.Conflict, message);
    public static OperationResult NotFound(string message) => new(false, OperationErrorType.NotFound, message);
    public static OperationResult Unauthorized(string message) => new(false, OperationErrorType.Unauthorized, message);
    public static OperationResult Unexpected(string? message = null) => new(false, OperationErrorType.Unexpected, message);
}

public record OperationResult<T>(bool Succeeded, T? Data, OperationErrorType ErrorType = OperationErrorType.None, string? ErrorMessage = null)
{
    public static OperationResult<T> Success(T data) => new(true, data);
    public static OperationResult<T> Invalid(string message) => new(false, default, OperationErrorType.InvalidInput, message);
    public static OperationResult<T> Conflict(string message) => new(false, default, OperationErrorType.Conflict, message);
    public static OperationResult<T> NotFound(string message) => new(false, default, OperationErrorType.NotFound, message);
    public static OperationResult<T> Unauthorized(string message) => new(false, default, OperationErrorType.Unauthorized, message);
    public static OperationResult<T> Unexpected(string? message = null) => new(false, default, OperationErrorType.Unexpected, message);
}
