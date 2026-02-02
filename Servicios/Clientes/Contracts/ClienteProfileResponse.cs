namespace Clientes.Contracts;

public record ClienteProfileResponse(
    Guid ClienteId,
    string Nombre,
    string Email,
    string Documento,
    Guid CuentaPrincipalId,
    Guid TarjetaPrincipalId);
