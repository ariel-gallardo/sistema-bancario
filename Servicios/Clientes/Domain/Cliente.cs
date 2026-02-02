namespace Clientes.Domain;

public class Cliente
{
    public Guid Id { get; set; }
    public required string NombreCompleto { get; set; }
    public required string Documento { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public Guid CuentaPrincipalId { get; set; }
    public Guid TarjetaPrincipalId { get; set; }
}
