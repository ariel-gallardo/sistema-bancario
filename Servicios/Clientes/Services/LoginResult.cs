using Clientes.Contracts;

namespace Clientes.Services;

public enum LoginErrorType
{
    None,
    InvalidInput,
    Unauthorized,
    Unknown
}

public sealed record LoginResult(LoginResponse? Response, LoginErrorType ErrorType, string? ErrorMessage)
{
    public bool Succeeded => ErrorType == LoginErrorType.None && Response is not null;

    public static LoginResult Success(LoginResponse response) => new(response, LoginErrorType.None, null);

    public static LoginResult InvalidInput(string message) => new(null, LoginErrorType.InvalidInput, message);

    public static LoginResult Unauthorized() => new(null, LoginErrorType.Unauthorized, null);

    public static LoginResult Failure(string? message) => new(null, LoginErrorType.Unknown, message);
}
