namespace Clientes.Domain;

public class ClienteRegistro
{
    public Guid Id { get; set; }
    public required string NombreCompleto { get; set; }
    public required string Documento { get; set; }
    public required string Email { get; set; }
    public required string Telefono { get; set; }
    public required string PasswordHash { get; set; }
    public required string Estado { get; set; }
    public DateTime CreadoEnUtc { get; set; }
}
