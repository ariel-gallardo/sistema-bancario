using Cuentas.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cuentas.Data;

public class CuentasDbInitializer(CuentasDbContext context)
{
    public static readonly Guid DemoClienteId = Guid.Parse("8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f");
    public static readonly Guid DemoCuentaPrincipalId = Guid.Parse("ca2f2eb2-e1a7-4fcf-9b19-63c19e78146a");
    public static readonly Guid DemoCuentaAhorroId = Guid.Parse("d4c8c7bb-ff69-4aa3-acf6-96e65e6a85ee");

    private readonly CuentasDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureEsFavoritaColumnAsync(cancellationToken);

        if (await _context.Cuentas.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        _context.Cuentas.AddRange(
            new CuentaBancaria
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
            new CuentaBancaria
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
            });

        await _context.SaveChangesAsync(cancellationToken);
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
}
