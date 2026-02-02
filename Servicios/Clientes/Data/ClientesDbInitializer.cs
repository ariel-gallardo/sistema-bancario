using Clientes.Domain;
using Clientes.Security;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Data;

public class ClientesDbInitializer(ClientesDbContext context)
{
    public static readonly Guid DemoClienteId = Guid.Parse("8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f");
    public static readonly Guid DemoCuentaId = Guid.Parse("ca2f2eb2-e1a7-4fcf-9b19-63c19e78146a");
    public static readonly Guid DemoTarjetaId = Guid.Parse("2c23c2a8-276a-4782-9a05-754e665d4e05");

    public static readonly Guid DemoClienteLucianaId = Guid.Parse("b1a4f90c-2cb9-4e9b-9294-1d812995ddc4");
    public static readonly Guid DemoCuentaLucianaId = Guid.Parse("f8f34b3a-bc2d-4fa7-9d52-6ad609b34464");
    public static readonly Guid DemoTarjetaLucianaId = Guid.Parse("a90df854-42f2-4bdd-9f9c-4a316ac2ed85");

    public static readonly Guid DemoClienteMateoId = Guid.Parse("0b1f22d1-f2cf-4e52-96d5-27be27fa9984");
    public static readonly Guid DemoCuentaMateoId = Guid.Parse("3f9f4034-00cc-4a4e-959f-e9b1b9bc572d");
    public static readonly Guid DemoTarjetaMateoId = Guid.Parse("6760c815-7f6d-46cc-8f3b-1f994af9f2ce");

    public static readonly Guid DemoClienteValentinaId = Guid.Parse("9f927feb-7761-4bd2-a6c5-2c4c7a1ccb79");
    public static readonly Guid DemoCuentaValentinaId = Guid.Parse("f54b7289-963b-4b77-86fe-52c57d20593d");
    public static readonly Guid DemoTarjetaValentinaId = Guid.Parse("5e0c463c-8e7b-42a5-afa0-f2434589c89a");

    private readonly ClientesDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureEsAdministradorColumnAsync(cancellationToken);
        await EnsureRegistroTableAsync(cancellationToken);

        var demoClientes = new List<Cliente>
        {
            new()
            {
                Id = DemoClienteId,
                NombreCompleto = "Ariel Gallardo",
                Documento = "30-12345678-9",
                Email = "demo@aurorabank.com",
                PasswordHash = PasswordHasher.Hash("B4nco$123"),
                CuentaPrincipalId = DemoCuentaId,
                TarjetaPrincipalId = DemoTarjetaId,
                EsAdministrador = true
            },
            new()
            {
                Id = DemoClienteLucianaId,
                NombreCompleto = "Luciana Ferraro",
                Documento = "27-29876543-5",
                Email = "luciana@aurorabank.com",
                PasswordHash = PasswordHasher.Hash("Admin#2024"),
                CuentaPrincipalId = DemoCuentaLucianaId,
                TarjetaPrincipalId = DemoTarjetaLucianaId,
                EsAdministrador = true
            },
            new()
            {
                Id = DemoClienteMateoId,
                NombreCompleto = "Mateo Rivas",
                Documento = "23-45788901-8",
                Email = "mateo@aurorabank.com",
                PasswordHash = PasswordHasher.Hash("Cliente#1"),
                CuentaPrincipalId = DemoCuentaMateoId,
                TarjetaPrincipalId = DemoTarjetaMateoId,
                EsAdministrador = false
            },
            new()
            {
                Id = DemoClienteValentinaId,
                NombreCompleto = "Valentina Duarte",
                Documento = "27-88651234-6",
                Email = "valentina@aurorabank.com",
                PasswordHash = PasswordHasher.Hash("Cliente#2"),
                CuentaPrincipalId = DemoCuentaValentinaId,
                TarjetaPrincipalId = DemoTarjetaValentinaId,
                EsAdministrador = false
            }
        };

        foreach (var cliente in demoClientes)
        {
            await UpsertClienteAsync(cliente, cancellationToken);
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureEsAdministradorColumnAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF COL_LENGTH('Clientes', 'EsAdministrador') IS NULL
            BEGIN
                ALTER TABLE [Clientes] ADD [EsAdministrador] bit NOT NULL CONSTRAINT DF_Clientes_EsAdministrador DEFAULT(0);
            END
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task EnsureRegistroTableAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF OBJECT_ID('ClienteRegistros', 'U') IS NULL
            BEGIN
                CREATE TABLE [ClienteRegistros]
                (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [NombreCompleto] nvarchar(160) NOT NULL,
                    [Documento] nvarchar(32) NOT NULL,
                    [Email] nvarchar(160) NOT NULL,
                    [Telefono] nvarchar(40) NOT NULL,
                    [PasswordHash] nvarchar(512) NOT NULL,
                    [Estado] nvarchar(32) NOT NULL CONSTRAINT DF_ClienteRegistros_Estado DEFAULT('Pendiente'),
                    [CreadoEnUtc] datetime2 NOT NULL
                );

                CREATE INDEX IX_ClienteRegistros_Email ON [ClienteRegistros]([Email]);
            END
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task UpsertClienteAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        var normalizedEmail = cliente.Email.Trim().ToLowerInvariant();
        cliente.Email = normalizedEmail;

        var existing = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == cliente.Id, cancellationToken);

        if (existing is null)
        {
            _context.Clientes.Add(cliente);
            return;
        }

        existing.NombreCompleto = cliente.NombreCompleto;
        existing.Documento = cliente.Documento;
        existing.Email = normalizedEmail;
        existing.PasswordHash = cliente.PasswordHash;
        existing.CuentaPrincipalId = cliente.CuentaPrincipalId;
        existing.TarjetaPrincipalId = cliente.TarjetaPrincipalId;
        existing.EsAdministrador = cliente.EsAdministrador;
    }
}
