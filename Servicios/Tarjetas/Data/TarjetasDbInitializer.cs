using Microsoft.EntityFrameworkCore;
using Tarjetas.Domain;

namespace Tarjetas.Data;

public class TarjetasDbInitializer(TarjetasDbContext context)
{
    public static readonly Guid DemoClienteId = Guid.Parse("8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f");
    public static readonly Guid DemoTarjetaId = Guid.Parse("2c23c2a8-276a-4782-9a05-754e665d4e05");

    private readonly TarjetasDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        if (await _context.Tarjetas.AnyAsync(cancellationToken))
        {
            return;
        }

        var tarjeta = new TarjetaCredito
        {
            Id = DemoTarjetaId,
            ClienteId = DemoClienteId,
            NumeroEnmascarado = "5234 **** **** 4412",
            Marca = "Visa Signature",
            Limite = 1800000m,
            SaldoUtilizado = 645230.75m,
            PagoMinimo = 90500.15m,
            Cierre = DateTime.UtcNow.AddDays(5),
            Vencimiento = DateTime.UtcNow.AddDays(15)
        };

        var ahora = DateTime.UtcNow;
        var movimientos = new List<MovimientoTarjeta>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                TarjetaId = tarjeta.Id,
                Comercio = "MercadoPress",
                Descripcion = "Suscripción plataforma financiera",
                Categoria = "Servicios",
                Importe = 18500.90m,
                Fecha = ahora.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                TarjetaId = tarjeta.Id,
                Comercio = "Aerolíneas del Sur",
                Descripcion = "Pasaje Córdoba - Mendoza",
                Categoria = "Viajes",
                Importe = 220000m,
                Fecha = ahora.AddDays(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                TarjetaId = tarjeta.Id,
                Comercio = "Gourmet 878",
                Descripcion = "Cena equipo",
                Categoria = "Restaurantes",
                Importe = 45200.45m,
                Fecha = ahora.AddDays(-4)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                TarjetaId = tarjeta.Id,
                Comercio = "Tech District",
                Descripcion = "Notebook Asus ProArt",
                Categoria = "Tecnología",
                Importe = 325999.99m,
                Fecha = ahora.AddDays(-8)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                TarjetaId = tarjeta.Id,
                Comercio = "Refuel Station 112",
                Descripcion = "Combustible YPF",
                Categoria = "Transporte",
                Importe = 41250.10m,
                Fecha = ahora.AddDays(-9)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClienteId = DemoClienteId,
                TarjetaId = tarjeta.Id,
                Comercio = "StreamingHub",
                Descripcion = "Plan familiar 4K",
                Categoria = "Servicios",
                Importe = 6899.99m,
                Fecha = ahora.AddDays(-11)
            }
        };

        _context.Tarjetas.Add(tarjeta);
        _context.Movimientos.AddRange(movimientos);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
