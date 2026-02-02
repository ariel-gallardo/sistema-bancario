using Clientes.Domain;
using Clientes.Security;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Data;

public class ClientesDbInitializer(ClientesDbContext context)
{
    public static readonly Guid DemoClienteId = Guid.Parse("8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f");
    public static readonly Guid DemoCuentaId = Guid.Parse("ca2f2eb2-e1a7-4fcf-9b19-63c19e78146a");
    public static readonly Guid DemoTarjetaId = Guid.Parse("2c23c2a8-276a-4782-9a05-754e665d4e05");

    private readonly ClientesDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        if (await _context.Clientes.AnyAsync(cancellationToken))
        {
            return;
        }

        _context.Clientes.Add(new Cliente
        {
            Id = DemoClienteId,
            NombreCompleto = "Ariel Gallardo",
            Documento = "30-12345678-9",
            Email = "demo@aurorabank.com",
            PasswordHash = PasswordHasher.Hash("B4nco$123"),
            CuentaPrincipalId = DemoCuentaId,
            TarjetaPrincipalId = DemoTarjetaId
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
