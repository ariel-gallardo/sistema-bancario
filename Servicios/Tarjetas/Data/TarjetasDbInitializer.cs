using Microsoft.EntityFrameworkCore;
using Tarjetas.Domain;

namespace Tarjetas.Data;

public class TarjetasDbInitializer(TarjetasDbContext context)
{
    public static readonly Guid DemoClienteId = Guid.Parse("8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f");
    public static readonly Guid DemoTarjetaId = Guid.Parse("2c23c2a8-276a-4782-9a05-754e665d4e05");

    public static readonly Guid DemoClienteLucianaId = Guid.Parse("b1a4f90c-2cb9-4e9b-9294-1d812995ddc4");
    public static readonly Guid DemoTarjetaLucianaId = Guid.Parse("a90df854-42f2-4bdd-9f9c-4a316ac2ed85");

    public static readonly Guid DemoClienteMateoId = Guid.Parse("0b1f22d1-f2cf-4e52-96d5-27be27fa9984");
    public static readonly Guid DemoTarjetaMateoId = Guid.Parse("6760c815-7f6d-46cc-8f3b-1f994af9f2ce");

    public static readonly Guid DemoClienteValentinaId = Guid.Parse("9f927feb-7761-4bd2-a6c5-2c4c7a1ccb79");
    public static readonly Guid DemoTarjetaValentinaId = Guid.Parse("5e0c463c-8e7b-42a5-afa0-f2434589c89a");

    private static readonly Guid DemoMovimientoMercadoPressId = Guid.Parse("0f1bba1f-6e2a-4a92-b266-4db3614d88af");
    private static readonly Guid DemoMovimientoAerolineasId = Guid.Parse("3f0d2b9c-63de-4ddb-8c62-2d58a3bfa931");
    private static readonly Guid DemoMovimientoGourmetId = Guid.Parse("093b99b7-5d5f-427c-b0f0-650ba9ee3f07");
    private static readonly Guid DemoMovimientoTechDistrictId = Guid.Parse("22f2a2f7-bf8a-4d7b-8c53-9f731f17df75");
    private static readonly Guid DemoMovimientoRefuelStationId = Guid.Parse("c3fc794f-11ed-4b4f-9232-4ccf06f5ec52");
    private static readonly Guid DemoMovimientoStreamingHubId = Guid.Parse("cda9cb11-8f90-4f31-8c6e-bbca9e5078e7");
    private static readonly Guid DemoMovimientoLogisticaId = Guid.Parse("8670f1db-1a58-4a54-8036-67c1bcd04ab3");
    private static readonly Guid DemoMovimientoHotelAltosId = Guid.Parse("c1cfce39-29b6-4ae9-87e5-87005a3f3814");
    private static readonly Guid DemoMovimientoSuministrosId = Guid.Parse("7da93771-5495-48c1-92de-5b287f98c2ab");
    private static readonly Guid DemoMovimientoMercadoCentralId = Guid.Parse("e56d2f07-2560-4b9a-8ce9-1cb31dc39449");
    private static readonly Guid DemoMovimientoPublicidadId = Guid.Parse("909f6723-1955-49bc-98f4-7da3c7eca46f");
    private static readonly Guid DemoMovimientoEstudioCreativoId = Guid.Parse("4b04fee3-362d-4fab-9f25-9db6466b7403");
    private static readonly Guid DemoMovimientoCoworkId = Guid.Parse("fd5d295c-249f-4bd8-a097-46bb296d801e");

    private readonly TarjetasDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        var ahora = DateTime.UtcNow;

        var tarjetas = new List<TarjetaCredito>
        {
            new()
            {
                Id = DemoTarjetaId,
                ClienteId = DemoClienteId,
                NumeroEnmascarado = "5234 **** **** 4412",
                Marca = "Visa Signature",
                Limite = 1800000m,
                SaldoUtilizado = 645230.75m,
                PagoMinimo = 90500.15m,
                Cierre = ahora.AddDays(5),
                Vencimiento = ahora.AddDays(15)
            },
            new()
            {
                Id = DemoTarjetaLucianaId,
                ClienteId = DemoClienteLucianaId,
                NumeroEnmascarado = "4899 **** **** 0021",
                Marca = "Mastercard Black",
                Limite = 2500000m,
                SaldoUtilizado = 1145230.10m,
                PagoMinimo = 142300.80m,
                Cierre = ahora.AddDays(3),
                Vencimiento = ahora.AddDays(13)
            },
            new()
            {
                Id = DemoTarjetaMateoId,
                ClienteId = DemoClienteMateoId,
                NumeroEnmascarado = "4780 **** **** 9901",
                Marca = "Visa Platinum",
                Limite = 850000m,
                SaldoUtilizado = 295430.55m,
                PagoMinimo = 48200.40m,
                Cierre = ahora.AddDays(6),
                Vencimiento = ahora.AddDays(16)
            },
            new()
            {
                Id = DemoTarjetaValentinaId,
                ClienteId = DemoClienteValentinaId,
                NumeroEnmascarado = "3759 **** **** 1122",
                Marca = "Amex Gold",
                Limite = 1200000m,
                SaldoUtilizado = 402780.25m,
                PagoMinimo = 60500.00m,
                Cierre = ahora.AddDays(4),
                Vencimiento = ahora.AddDays(14)
            }
        };

        var movimientos = new List<MovimientoTarjeta>
        {
            new()
            {
                Id = DemoMovimientoMercadoPressId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaId,
                Comercio = "MercadoPress",
                Descripcion = "Suscripción plataforma financiera",
                Categoria = "Servicios",
                Importe = 18500.90m,
                Fecha = ahora.AddDays(-1)
            },
            new()
            {
                Id = DemoMovimientoAerolineasId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaId,
                Comercio = "Aerolíneas del Sur",
                Descripcion = "Pasaje Córdoba - Mendoza",
                Categoria = "Viajes",
                Importe = 220000m,
                Fecha = ahora.AddDays(-3)
            },
            new()
            {
                Id = DemoMovimientoGourmetId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaId,
                Comercio = "Gourmet 878",
                Descripcion = "Cena equipo",
                Categoria = "Restaurantes",
                Importe = 45200.45m,
                Fecha = ahora.AddDays(-4)
            },
            new()
            {
                Id = DemoMovimientoTechDistrictId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaId,
                Comercio = "Tech District",
                Descripcion = "Notebook Asus ProArt",
                Categoria = "Tecnología",
                Importe = 325999.99m,
                Fecha = ahora.AddDays(-8)
            },
            new()
            {
                Id = DemoMovimientoRefuelStationId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaId,
                Comercio = "Refuel Station 112",
                Descripcion = "Combustible YPF",
                Categoria = "Transporte",
                Importe = 41250.10m,
                Fecha = ahora.AddDays(-9)
            },
            new()
            {
                Id = DemoMovimientoStreamingHubId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaId,
                Comercio = "StreamingHub",
                Descripcion = "Plan familiar 4K",
                Categoria = "Servicios",
                Importe = 6899.99m,
                Fecha = ahora.AddDays(-11)
            },
            new()
            {
                Id = DemoMovimientoLogisticaId,
                ClienteId = DemoClienteLucianaId,
                TarjetaId = DemoTarjetaLucianaId,
                Comercio = "Logística Federal",
                Descripcion = "Flete urgente",
                Categoria = "Operaciones",
                Importe = 185000.00m,
                Fecha = ahora.AddDays(-2)
            },
            new()
            {
                Id = DemoMovimientoHotelAltosId,
                ClienteId = DemoClienteLucianaId,
                TarjetaId = DemoTarjetaLucianaId,
                Comercio = "Hotel Altos Andes",
                Descripcion = "Reserva para comité",
                Categoria = "Viajes",
                Importe = 312450.75m,
                Fecha = ahora.AddDays(-5)
            },
            new()
            {
                Id = DemoMovimientoSuministrosId,
                ClienteId = DemoClienteLucianaId,
                TarjetaId = DemoTarjetaLucianaId,
                Comercio = "Suministros SRL",
                Descripcion = "Insumos TI",
                Categoria = "Tecnología",
                Importe = 222199.35m,
                Fecha = ahora.AddDays(-7)
            },
            new()
            {
                Id = DemoMovimientoMercadoCentralId,
                ClienteId = DemoClienteMateoId,
                TarjetaId = DemoTarjetaMateoId,
                Comercio = "MercadoCentral",
                Descripcion = "Reposición mercadería",
                Categoria = "Inventario",
                Importe = 85500.60m,
                Fecha = ahora.AddDays(-1)
            },
            new()
            {
                Id = DemoMovimientoPublicidadId,
                ClienteId = DemoClienteMateoId,
                TarjetaId = DemoTarjetaMateoId,
                Comercio = "Publicidad Nube",
                Descripcion = "Campaña digital",
                Categoria = "Marketing",
                Importe = 48220.00m,
                Fecha = ahora.AddDays(-4)
            },
            new()
            {
                Id = DemoMovimientoEstudioCreativoId,
                ClienteId = DemoClienteValentinaId,
                TarjetaId = DemoTarjetaValentinaId,
                Comercio = "Estudio Creativo 9R",
                Descripcion = "Producción audiovisual",
                Categoria = "Servicios",
                Importe = 126800.00m,
                Fecha = ahora.AddDays(-3)
            },
            new()
            {
                Id = DemoMovimientoCoworkId,
                ClienteId = DemoClienteValentinaId,
                TarjetaId = DemoTarjetaValentinaId,
                Comercio = "Cowork Andén",
                Descripcion = "Alquiler salas",
                Categoria = "Oficinas",
                Importe = 32500.15m,
                Fecha = ahora.AddDays(-6)
            }
        };

        foreach (var tarjeta in tarjetas)
        {
            await UpsertTarjetaAsync(tarjeta, cancellationToken);
        }

        foreach (var movimiento in movimientos)
        {
            await UpsertMovimientoAsync(movimiento, cancellationToken);
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task UpsertTarjetaAsync(TarjetaCredito tarjeta, CancellationToken cancellationToken)
    {
        var existing = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == tarjeta.Id, cancellationToken);

        if (existing is null)
        {
            _context.Tarjetas.Add(tarjeta);
            return;
        }

        existing.ClienteId = tarjeta.ClienteId;
        existing.NumeroEnmascarado = tarjeta.NumeroEnmascarado;
        existing.Marca = tarjeta.Marca;
        existing.Limite = tarjeta.Limite;
        existing.SaldoUtilizado = tarjeta.SaldoUtilizado;
        existing.PagoMinimo = tarjeta.PagoMinimo;
        existing.Cierre = tarjeta.Cierre;
        existing.Vencimiento = tarjeta.Vencimiento;
    }

    private async Task UpsertMovimientoAsync(MovimientoTarjeta movimiento, CancellationToken cancellationToken)
    {
        var existing = await _context.Movimientos.FirstOrDefaultAsync(m => m.Id == movimiento.Id, cancellationToken);

        if (existing is null)
        {
            _context.Movimientos.Add(movimiento);
            return;
        }

        existing.ClienteId = movimiento.ClienteId;
        existing.TarjetaId = movimiento.TarjetaId;
        existing.Comercio = movimiento.Comercio;
        existing.Descripcion = movimiento.Descripcion;
        existing.Importe = movimiento.Importe;
        existing.Fecha = movimiento.Fecha;
        existing.Categoria = movimiento.Categoria;
    }
}
