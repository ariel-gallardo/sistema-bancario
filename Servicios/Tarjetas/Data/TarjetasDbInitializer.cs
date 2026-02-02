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

    private static readonly Guid DemoCuentaArielPrincipalId = Guid.Parse("ca2f2eb2-e1a7-4fcf-9b19-63c19e78146a");
    private static readonly Guid DemoCuentaArielAhorroId = Guid.Parse("d4c8c7bb-ff69-4aa3-acf6-96e65e6a85ee");

    private static readonly Guid DemoCuentaLucianaPrincipalId = Guid.Parse("f8f34b3a-bc2d-4fa7-9d52-6ad609b34464");
    private static readonly Guid DemoCuentaLucianaInversionId = Guid.Parse("1f7d6af0-3e92-4a5b-ac50-973360829421");

    private static readonly Guid DemoCuentaMateoPrincipalId = Guid.Parse("3f9f4034-00cc-4a4e-959f-e9b1b9bc572d");
    private static readonly Guid DemoCuentaMateoAhorroId = Guid.Parse("b6b6010a-0bea-45df-9ea3-c3241653df15");

    private static readonly Guid DemoCuentaValentinaPrincipalId = Guid.Parse("f54b7289-963b-4b77-86fe-52c57d20593d");
    private static readonly Guid DemoCuentaValentinaPremiumId = Guid.Parse("41f1cbbd-1f35-41da-b1aa-6f5e6a1d9d68");

    private static readonly Guid DemoTarjetaArielMasterId = Guid.Parse("f8d7a4b1-748b-45df-b6f0-7d4d65d8b5af");
    private static readonly Guid DemoTarjetaArielVisaUsdId = Guid.Parse("5a2d02ef-3e5c-484c-8346-6db1cfd5a4bd");

    private static readonly Guid DemoTarjetaLucianaVisaInfiniteId = Guid.Parse("f1a2d3c4-5b6c-4d7e-8f90-1234abcd5678");
    private static readonly Guid DemoTarjetaLucianaCorporateId = Guid.Parse("0c52d04d-9a78-4a31-bc8a-1eac4c8f2a57");

    private static readonly Guid DemoTarjetaMateoBusinessId = Guid.Parse("6e92b8ae-92f6-4bd5-83b3-5f76df55d6e0");
    private static readonly Guid DemoTarjetaMateoMasterId = Guid.Parse("a328aaf8-0d61-4dfc-acd1-9cba9d8e1f0c");

    private static readonly Guid DemoTarjetaValentinaVisaId = Guid.Parse("9eb24db7-9ad3-4ecb-8d9c-cc9b6b8320d4");
    private static readonly Guid DemoTarjetaValentinaMasterId = Guid.Parse("0b0e5c4c-32c4-4b99-b1ab-6cfaae2b4977");

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
    private static readonly Guid DemoMovimientoSolarTechId = Guid.Parse("1d5c8b7a-7f44-4d97-8dd2-2d2281183d5b");
    private static readonly Guid DemoMovimientoCargoPlusId = Guid.Parse("5c2e3a44-2c7e-4d51-9aef-0d26f3c841c8");
    private static readonly Guid DemoMovimientoWellnessLabId = Guid.Parse("6f4df9c8-5fdb-4d9d-8d4e-942eaf0da963");
    private static readonly Guid DemoMovimientoRetailCloudId = Guid.Parse("959d3d74-2fe1-46df-9bcc-02ec1c45f65e");
    private static readonly Guid DemoMovimientoHelixStudioId = Guid.Parse("a11b9d04-6f68-4e54-8d89-6337354d8431");
    private static readonly Guid DemoMovimientoDigitalAdsId = Guid.Parse("c86a9fb7-053c-4b0f-9a6c-3803bf6d3b77");

    private readonly TarjetasDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureCuentaIdColumnAsync(cancellationToken);
        await BackfillCuentaIdsAsync(cancellationToken);

        var ahora = DateTime.UtcNow;

        var tarjetas = new List<TarjetaCredito>
        {
            new()
            {
                Id = DemoTarjetaId,
                ClienteId = DemoClienteId,
                CuentaId = DemoCuentaArielPrincipalId,
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
                Id = DemoTarjetaArielMasterId,
                ClienteId = DemoClienteId,
                CuentaId = DemoCuentaArielPrincipalId,
                NumeroEnmascarado = "5333 **** **** 0912",
                Marca = "Mastercard Black",
                Limite = 2200000m,
                SaldoUtilizado = 1250999.45m,
                PagoMinimo = 131500.00m,
                Cierre = ahora.AddDays(7),
                Vencimiento = ahora.AddDays(17)
            },
            new()
            {
                Id = DemoTarjetaArielVisaUsdId,
                ClienteId = DemoClienteId,
                CuentaId = DemoCuentaArielAhorroId,
                NumeroEnmascarado = "4550 **** **** 7710",
                Marca = "Visa Platinum USD",
                Limite = 65000m,
                SaldoUtilizado = 18250.30m,
                PagoMinimo = 4200.00m,
                Cierre = ahora.AddDays(8),
                Vencimiento = ahora.AddDays(18)
            },
            new()
            {
                Id = DemoTarjetaLucianaId,
                ClienteId = DemoClienteLucianaId,
                CuentaId = DemoCuentaLucianaPrincipalId,
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
                Id = DemoTarjetaLucianaVisaInfiniteId,
                ClienteId = DemoClienteLucianaId,
                CuentaId = DemoCuentaLucianaPrincipalId,
                NumeroEnmascarado = "4512 **** **** 6601",
                Marca = "Visa Infinite",
                Limite = 3200000m,
                SaldoUtilizado = 1540023.75m,
                PagoMinimo = 210000.00m,
                Cierre = ahora.AddDays(4),
                Vencimiento = ahora.AddDays(14)
            },
            new()
            {
                Id = DemoTarjetaLucianaCorporateId,
                ClienteId = DemoClienteLucianaId,
                CuentaId = DemoCuentaLucianaInversionId,
                NumeroEnmascarado = "5340 **** **** 2201",
                Marca = "Mastercard Corporate USD",
                Limite = 120000m,
                SaldoUtilizado = 56220.10m,
                PagoMinimo = 8600.00m,
                Cierre = ahora.AddDays(6),
                Vencimiento = ahora.AddDays(16)
            },
            new()
            {
                Id = DemoTarjetaMateoId,
                ClienteId = DemoClienteMateoId,
                CuentaId = DemoCuentaMateoPrincipalId,
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
                Id = DemoTarjetaMateoBusinessId,
                ClienteId = DemoClienteMateoId,
                CuentaId = DemoCuentaMateoPrincipalId,
                NumeroEnmascarado = "4921 **** **** 3105",
                Marca = "Visa Business",
                Limite = 540000m,
                SaldoUtilizado = 210330.45m,
                PagoMinimo = 35600.00m,
                Cierre = ahora.AddDays(2),
                Vencimiento = ahora.AddDays(12)
            },
            new()
            {
                Id = DemoTarjetaMateoMasterId,
                ClienteId = DemoClienteMateoId,
                CuentaId = DemoCuentaMateoAhorroId,
                NumeroEnmascarado = "5252 **** **** 4420",
                Marca = "Mastercard Platinum",
                Limite = 280000m,
                SaldoUtilizado = 118500.35m,
                PagoMinimo = 22400.00m,
                Cierre = ahora.AddDays(9),
                Vencimiento = ahora.AddDays(19)
            },
            new()
            {
                Id = DemoTarjetaValentinaId,
                ClienteId = DemoClienteValentinaId,
                CuentaId = DemoCuentaValentinaPrincipalId,
                NumeroEnmascarado = "3759 **** **** 1122",
                Marca = "Amex Gold",
                Limite = 1200000m,
                SaldoUtilizado = 402780.25m,
                PagoMinimo = 60500.00m,
                Cierre = ahora.AddDays(4),
                Vencimiento = ahora.AddDays(14)
            },
            new()
            {
                Id = DemoTarjetaValentinaVisaId,
                ClienteId = DemoClienteValentinaId,
                CuentaId = DemoCuentaValentinaPrincipalId,
                NumeroEnmascarado = "4220 **** **** 7722",
                Marca = "Visa Signature",
                Limite = 900000m,
                SaldoUtilizado = 385220.15m,
                PagoMinimo = 50200.00m,
                Cierre = ahora.AddDays(5),
                Vencimiento = ahora.AddDays(15)
            },
            new()
            {
                Id = DemoTarjetaValentinaMasterId,
                ClienteId = DemoClienteValentinaId,
                CuentaId = DemoCuentaValentinaPremiumId,
                NumeroEnmascarado = "5318 **** **** 3588",
                Marca = "Mastercard Titanium",
                Limite = 450000m,
                SaldoUtilizado = 152400.00m,
                PagoMinimo = 28700.00m,
                Cierre = ahora.AddDays(1),
                Vencimiento = ahora.AddDays(11)
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
            },
            new()
            {
                Id = DemoMovimientoSolarTechId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaArielMasterId,
                Comercio = "Solar Tech Labs",
                Descripcion = "Paneles solares ejecutivos",
                Categoria = "Infraestructura",
                Importe = 410000.00m,
                Fecha = ahora.AddDays(-2)
            },
            new()
            {
                Id = DemoMovimientoCargoPlusId,
                ClienteId = DemoClienteId,
                TarjetaId = DemoTarjetaArielVisaUsdId,
                Comercio = "CargoPlus Intl",
                Descripcion = "Flete aéreo urgente",
                Categoria = "Logística",
                Importe = 8900.00m,
                Fecha = ahora.AddDays(-10)
            },
            new()
            {
                Id = DemoMovimientoWellnessLabId,
                ClienteId = DemoClienteLucianaId,
                TarjetaId = DemoTarjetaLucianaVisaInfiniteId,
                Comercio = "Wellness Lab",
                Descripcion = "Bienestar comité ejecutivo",
                Categoria = "Beneficios",
                Importe = 68500.00m,
                Fecha = ahora.AddDays(-4)
            },
            new()
            {
                Id = DemoMovimientoRetailCloudId,
                ClienteId = DemoClienteLucianaId,
                TarjetaId = DemoTarjetaLucianaCorporateId,
                Comercio = "Retail Cloud",
                Descripcion = "Almacenamiento escalable",
                Categoria = "Tecnología",
                Importe = 31200.75m,
                Fecha = ahora.AddDays(-9)
            },
            new()
            {
                Id = DemoMovimientoHelixStudioId,
                ClienteId = DemoClienteMateoId,
                TarjetaId = DemoTarjetaMateoBusinessId,
                Comercio = "Helix Studio",
                Descripcion = "Branding local",
                Categoria = "Marketing",
                Importe = 26500.00m,
                Fecha = ahora.AddDays(-7)
            },
            new()
            {
                Id = DemoMovimientoDigitalAdsId,
                ClienteId = DemoClienteValentinaId,
                TarjetaId = DemoTarjetaValentinaMasterId,
                Comercio = "Digital Ads Hub",
                Descripcion = "Campaña multiplataforma",
                Categoria = "Publicidad",
                Importe = 74200.00m,
                Fecha = ahora.AddDays(-5)
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

    private async Task EnsureCuentaIdColumnAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF COL_LENGTH('Tarjetas', 'CuentaId') IS NULL
            BEGIN
                ALTER TABLE [Tarjetas] ADD [CuentaId] uniqueidentifier NULL;
            END
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task BackfillCuentaIdsAsync(CancellationToken cancellationToken)
    {
        var sql = $"""
            UPDATE T
            SET CuentaId = CASE T.Id
                WHEN '{DemoTarjetaId}' THEN '{DemoCuentaArielPrincipalId}'
                WHEN '{DemoTarjetaLucianaId}' THEN '{DemoCuentaLucianaPrincipalId}'
                WHEN '{DemoTarjetaMateoId}' THEN '{DemoCuentaMateoPrincipalId}'
                WHEN '{DemoTarjetaValentinaId}' THEN '{DemoCuentaValentinaPrincipalId}'
                ELSE T.CuentaId
            END
            FROM [Tarjetas] AS T
            WHERE T.CuentaId IS NULL
        """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
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
        existing.CuentaId = tarjeta.CuentaId;
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
