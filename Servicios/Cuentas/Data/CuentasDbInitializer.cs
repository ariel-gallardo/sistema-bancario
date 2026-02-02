using Cuentas.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cuentas.Data;

public class CuentasDbInitializer(CuentasDbContext context)
{
    public static readonly Guid DemoClienteId = Guid.Parse("8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f");
    public static readonly Guid DemoCuentaPrincipalId = Guid.Parse("ca2f2eb2-e1a7-4fcf-9b19-63c19e78146a");
    public static readonly Guid DemoCuentaAhorroId = Guid.Parse("d4c8c7bb-ff69-4aa3-acf6-96e65e6a85ee");

    public static readonly Guid DemoClienteLucianaId = Guid.Parse("b1a4f90c-2cb9-4e9b-9294-1d812995ddc4");
    public static readonly Guid DemoCuentaLucianaPrincipalId = Guid.Parse("f8f34b3a-bc2d-4fa7-9d52-6ad609b34464");
    public static readonly Guid DemoCuentaLucianaInversionId = Guid.Parse("1f7d6af0-3e92-4a5b-ac50-973360829421");

    public static readonly Guid DemoClienteMateoId = Guid.Parse("0b1f22d1-f2cf-4e52-96d5-27be27fa9984");
    public static readonly Guid DemoCuentaMateoPrincipalId = Guid.Parse("3f9f4034-00cc-4a4e-959f-e9b1b9bc572d");
    public static readonly Guid DemoCuentaMateoAhorroId = Guid.Parse("b6b6010a-0bea-45df-9ea3-c3241653df15");

    public static readonly Guid DemoClienteValentinaId = Guid.Parse("9f927feb-7761-4bd2-a6c5-2c4c7a1ccb79");
    public static readonly Guid DemoCuentaValentinaPrincipalId = Guid.Parse("f54b7289-963b-4b77-86fe-52c57d20593d");
    public static readonly Guid DemoCuentaValentinaPremiumId = Guid.Parse("41f1cbbd-1f35-41da-b1aa-6f5e6a1d9d68");

    private readonly CuentasDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureEsFavoritaColumnAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var cuentas = new List<CuentaBancaria>
        {
            new()
            {
                Id = DemoCuentaPrincipalId,
                ClienteId = DemoClienteId,
                Alias = "aurora.sueldo.ar",
                Banco = "Aurora Bank",
                Numero = "0001-343434/7",
                Moneda = "ARS",
                SaldoActual = 875000.25m,
                LimiteDescubierto = 100000m,
                EsPrincipal = true,
                EsFavorita = true,
                UltimaActualizacion = now
            },
            new()
            {
                Id = DemoCuentaAhorroId,
                ClienteId = DemoClienteId,
                Alias = "aurora.ahorro.usd",
                Banco = "Aurora Bank",
                Numero = "0001-565655/1",
                Moneda = "USD",
                SaldoActual = 3250.40m,
                LimiteDescubierto = 0,
                EsPrincipal = false,
                EsFavorita = false,
                UltimaActualizacion = now.AddMinutes(-12)
            },
            new()
            {
                Id = DemoCuentaLucianaPrincipalId,
                ClienteId = DemoClienteLucianaId,
                Alias = "aurora.operaciones.ar",
                Banco = "Aurora Bank",
                Numero = "0002-112200/3",
                Moneda = "ARS",
                SaldoActual = 1625000.80m,
                LimiteDescubierto = 250000m,
                EsPrincipal = true,
                EsFavorita = true,
                UltimaActualizacion = now.AddMinutes(-3)
            },
            new()
            {
                Id = DemoCuentaLucianaInversionId,
                ClienteId = DemoClienteLucianaId,
                Alias = "aurora.ops.usd",
                Banco = "Aurora Bank",
                Numero = "0002-778899/5",
                Moneda = "USD",
                SaldoActual = 98500.45m,
                LimiteDescubierto = 0,
                EsPrincipal = false,
                EsFavorita = false,
                UltimaActualizacion = now.AddMinutes(-25)
            },
            new()
            {
                Id = DemoCuentaMateoPrincipalId,
                ClienteId = DemoClienteMateoId,
                Alias = "aurora.servicios.ar",
                Banco = "Aurora Bank",
                Numero = "0003-445566/9",
                Moneda = "ARS",
                SaldoActual = 245000.10m,
                LimiteDescubierto = 40000m,
                EsPrincipal = true,
                EsFavorita = true,
                UltimaActualizacion = now.AddMinutes(-6)
            },
            new()
            {
                Id = DemoCuentaMateoAhorroId,
                ClienteId = DemoClienteMateoId,
                Alias = "aurora.servicios.ahorro",
                Banco = "Aurora Bank",
                Numero = "0003-889900/1",
                Moneda = "ARS",
                SaldoActual = 78500.00m,
                LimiteDescubierto = 0,
                EsPrincipal = false,
                EsFavorita = false,
                UltimaActualizacion = now.AddMinutes(-44)
            },
            new()
            {
                Id = DemoCuentaValentinaPrincipalId,
                ClienteId = DemoClienteValentinaId,
                Alias = "aurora.creativa.ar",
                Banco = "Aurora Bank",
                Numero = "0004-667788/2",
                Moneda = "ARS",
                SaldoActual = 512340.65m,
                LimiteDescubierto = 80000m,
                EsPrincipal = true,
                EsFavorita = true,
                UltimaActualizacion = now.AddMinutes(-9)
            },
            new()
            {
                Id = DemoCuentaValentinaPremiumId,
                ClienteId = DemoClienteValentinaId,
                Alias = "aurora.creativa.usd",
                Banco = "Aurora Bank",
                Numero = "0004-998877/0",
                Moneda = "USD",
                SaldoActual = 15320.90m,
                LimiteDescubierto = 0,
                EsPrincipal = false,
                EsFavorita = false,
                UltimaActualizacion = now.AddMinutes(-61)
            }
        };

        foreach (var cuenta in cuentas)
        {
            await UpsertCuentaAsync(cuenta, cancellationToken);
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureEsFavoritaColumnAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF COL_LENGTH('Cuentas', 'EsFavorita') IS NULL
            BEGIN
                ALTER TABLE [Cuentas] ADD [EsFavorita] bit NOT NULL CONSTRAINT DF_Cuentas_EsFavorita DEFAULT(0);
            END
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task UpsertCuentaAsync(CuentaBancaria cuenta, CancellationToken cancellationToken)
    {
        var existing = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == cuenta.Id, cancellationToken);

        if (existing is null)
        {
            _context.Cuentas.Add(cuenta);
            return;
        }

        existing.ClienteId = cuenta.ClienteId;
        existing.Alias = cuenta.Alias;
        existing.Banco = cuenta.Banco;
        existing.Numero = cuenta.Numero;
        existing.Moneda = cuenta.Moneda;
        existing.SaldoActual = cuenta.SaldoActual;
        existing.LimiteDescubierto = cuenta.LimiteDescubierto;
        existing.EsPrincipal = cuenta.EsPrincipal;
        existing.EsFavorita = cuenta.EsFavorita;
        existing.UltimaActualizacion = cuenta.UltimaActualizacion;
    }
}
