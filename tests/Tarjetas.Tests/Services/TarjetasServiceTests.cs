using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Tarjetas.Contracts;
using Tarjetas.Data;
using Tarjetas.Domain;
using Tarjetas.Infrastructure;
using Tarjetas.Services;
using Xunit;

namespace Tarjetas.Tests.Services;

public class TarjetasServiceTests
{
    [Fact]
    public async Task GetPrincipalMovimientosAsync_UsesPreferredTarjetaAndOrdersMovements()
    {
        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();

        await using var context = CreateDbContext();
        await SeedTarjetaAsync(context, tarjetaId, clienteId, cuentaId: Guid.NewGuid(), saldoUtilizado: 1000m);
        await SeedMovimientosAsync(context, tarjetaId, clienteId);

        var userContext = new TestUserContext(clienteId, tarjetaId, false);
        var service = new TarjetasService(context, userContext);

        var result = await service.GetPrincipalMovimientosAsync(take: 3, CancellationToken.None);

        var okResult = Assert.IsType<Ok<TarjetaMovimientosResponse>>(result);
        var payload = okResult.Value;
        payload.Should().NotBeNull();
        payload!.Movimientos.Should().HaveCount(3);
        payload.Movimientos.Should().BeInDescendingOrder(m => m.Fecha);
    }

    [Fact]
    public async Task GetMovimientosByTarjetaAsync_ReturnsNotFoundWhenCardDoesNotBelongToUser()
    {
        var cardOwnerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();

        await using var context = CreateDbContext();
        await SeedTarjetaAsync(context, tarjetaId, cardOwnerId, Guid.NewGuid(), 500m);

        var userContext = new TestUserContext(requesterId, null, false);
        var service = new TarjetasService(context, userContext);

        var result = await service.GetMovimientosByTarjetaAsync(tarjetaId, take: null, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        statusResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AddMovimientoAsync_AdminUserPersistsMovementAndUpdatesBalance()
    {
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        await using var context = CreateDbContext();
        await SeedTarjetaAsync(context, tarjetaId, clienteId, Guid.NewGuid(), saldoUtilizado: 500m);

        var userContext = new TestUserContext(clienteId, tarjetaId, true);
        var service = new TarjetasService(context, userContext);

        var request = new CrearMovimientoRequest("MercadoPress", "Suscripción", "Servicios", 250m, DateTime.UtcNow);

        var result = await service.AddMovimientoAsync(tarjetaId, request, CancellationToken.None);

        var createdResult = Assert.IsType<Created<MovimientoTarjetaResponse>>(result);
        createdResult.Location.Should().Contain(tarjetaId.ToString());

        var storedTarjeta = await context.Tarjetas.AsNoTracking().FirstAsync(t => t.Id == tarjetaId);
        storedTarjeta.SaldoUtilizado.Should().Be(750m);

        var movimiento = await context.Movimientos.AsNoTracking().FirstOrDefaultAsync(m => m.TarjetaId == tarjetaId && m.Comercio == "MercadoPress");
        movimiento.Should().NotBeNull();
        movimiento!.Importe.Should().Be(250m);
    }

    private static TarjetasDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TarjetasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TarjetasDbContext(options);
    }

    private static async Task SeedTarjetaAsync(
        TarjetasDbContext context,
        Guid tarjetaId,
        Guid clienteId,
        Guid cuentaId,
        decimal saldoUtilizado)
    {
        context.Tarjetas.Add(new TarjetaCredito
        {
            Id = tarjetaId,
            ClienteId = clienteId,
            CuentaId = cuentaId,
            NumeroEnmascarado = "4550 **** **** 7710",
            Marca = "Visa Platinum",
            Limite = 10000m,
            SaldoUtilizado = saldoUtilizado,
            PagoMinimo = 100m,
            Cierre = DateTime.UtcNow.AddDays(5),
            Vencimiento = DateTime.UtcNow.AddDays(15)
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedMovimientosAsync(TarjetasDbContext context, Guid tarjetaId, Guid clienteId)
    {
        var eventos = Enumerable.Range(0, 4)
            .Select(offset => new MovimientoTarjeta
            {
                Id = Guid.NewGuid(),
                TarjetaId = tarjetaId,
                ClienteId = clienteId,
                Comercio = $"Comercio {offset}",
                Descripcion = "Cargo recurrente",
                Categoria = "Servicios",
                Importe = 1000m + offset,
                Fecha = DateTime.UtcNow.AddDays(-offset)
            });

        context.Movimientos.AddRange(eventos);
        await context.SaveChangesAsync();
    }

    private sealed class TestUserContext : IUserContext
    {
        public TestUserContext(Guid? clienteId, Guid? tarjetaPrincipalId, bool esAdministrador)
        {
            ClienteId = clienteId;
            TarjetaPrincipalId = tarjetaPrincipalId;
            EsAdministrador = esAdministrador;
        }

        public Guid? ClienteId { get; }
        public Guid? TarjetaPrincipalId { get; }
        public bool EsAdministrador { get; }
    }
}
