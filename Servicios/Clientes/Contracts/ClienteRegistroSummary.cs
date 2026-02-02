namespace Clientes.Contracts;

public record ClienteRegistroSummary(
    Guid RegistroId,
    string NombreCompleto,
    string Documento,
    string Email,
    string Telefono,
    string Estado,
    DateTime CreadoEnUtc);
