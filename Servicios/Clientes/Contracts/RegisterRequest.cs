namespace Clientes.Contracts;

public record RegisterRequest(
    string NombreCompleto,
    string Documento,
    string Email,
    string Telefono,
    string Clave);
