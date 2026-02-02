using Microsoft.EntityFrameworkCore;
using Pagos.Domain;

namespace Pagos.Data;

public class PagosDbInitializer(PagosDbContext context)
{
    public static readonly Guid DemoClienteId = Guid.Parse("8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f");

    private readonly PagosDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        if (await _context.PagosProgramados.AnyAsync(cancellationToken))
        {
            return;
        }

        var ahora = DateTime.UtcNow;

        var programados = new List<PagoProgramado>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                Empresa = "Aurora Seguros",
                Descripcion = "Cobertura hogar premium",
                Importe = 185000.45m,
                Moneda = "ARS",
                FechaProgramada = ahora.AddDays(2),
                EsDebitoAutomatico = true,
                Estado = "Pendiente"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                Empresa = "Cooperativa Solar Andina",
                Descripcion = "Servicio de energía",
                Importe = 52340.10m,
                Moneda = "ARS",
                FechaProgramada = ahora.AddDays(4),
                EsDebitoAutomatico = true,
                Estado = "Pendiente"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                Empresa = "StreamPlus",
                Descripcion = "Suscripción corporativa",
                Importe = 11999.00m,
                Moneda = "ARS",
                FechaProgramada = ahora.AddDays(6),
                EsDebitoAutomatico = false,
                Estado = "Pendiente"
            }
        };

        var historico = new List<PagoHistorico>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                Empresa = "Metropolitana de Servicios",
                Categoria = "Telecomunicaciones",
                Importe = 86500.25m,
                Moneda = "ARS",
                FechaPago = ahora.AddDays(-2),
                MedioPago = "Débito automático",
                FueDebitoAutomatico = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                Empresa = "Consorcio Artigas 878",
                Categoria = "Servicios",
                Importe = 45200.00m,
                Moneda = "ARS",
                FechaPago = ahora.AddDays(-5),
                MedioPago = "Transferencia inmediata",
                FueDebitoAutomatico = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                Empresa = "Enargas",
                Categoria = "Energía",
                Importe = 73220.75m,
                Moneda = "ARS",
                FechaPago = ahora.AddDays(-8),
                MedioPago = "Débito automático",
                FueDebitoAutomatico = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                Empresa = "Aysa",
                Categoria = "Agua",
                Importe = 21250.30m,
                Moneda = "ARS",
                FechaPago = ahora.AddDays(-10),
                MedioPago = "Transferencia inmediata",
                FueDebitoAutomatico = false
            }
        };

        _context.PagosProgramados.AddRange(programados);
        _context.PagosHistoricos.AddRange(historico);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
