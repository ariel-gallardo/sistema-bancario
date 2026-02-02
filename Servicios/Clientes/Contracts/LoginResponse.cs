namespace Clientes.Contracts;

public record LoginResponse(
    Guid ClienteId,
    Guid CuentaPrincipalId,
    Guid TarjetaPrincipalId,
    string Nombre,
    string Email,
    string Token,
    DateTime ExpiraUtc);
