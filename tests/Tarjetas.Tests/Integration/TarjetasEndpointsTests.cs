using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tarjetas.Contracts;
using Tarjetas.Data;
using Tarjetas.Domain;
using Tarjetas.Tests.Infrastructure;
using Xunit;

namespace Tarjetas.Tests.Integration;

public class TarjetasEndpointsTests : IClassFixture<TarjetasWebApplicationFactory>
{
    private readonly TarjetasWebApplicationFactory _factory;

    public TarjetasEndpointsTests(TarjetasWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPrincipalMovimientos_ReturnsLimitedOrderedMovements()
    {
        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var client = _factory.CreateClient();

        await _factory.WithDataAsync(context =>
        {
            context.Tarjetas.Add(CreateTarjeta(tarjetaId, clienteId, Guid.NewGuid(), saldoUtilizado: 1000m));
            context.Movimientos.AddRange(CreateMovimientos(tarjetaId, clienteId, 4));
            return Task.CompletedTask;
        });

        AddIdentityHeaders(client, clienteId, tarjetaId, isAdmin: false);

        var response = await client.GetAsync("/api/tarjetas/principal/movimientos?take=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<TarjetaMovimientosResponse>();
        payload.Should().NotBeNull();
        payload!.Movimientos.Should().HaveCount(2);
        payload.Movimientos.Should().BeInDescendingOrder(m => m.Fecha);
    }

    [Fact]
    public async Task PostMovimiento_AsAdminCreatesMovement()
    {
        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var client = _factory.CreateClient();

        await _factory.WithDataAsync(context =>
        {
            context.Tarjetas.Add(CreateTarjeta(tarjetaId, clienteId, Guid.NewGuid(), saldoUtilizado: 250m));
            return Task.CompletedTask;
        });

        AddIdentityHeaders(client, clienteId, tarjetaId, isAdmin: true);

        var request = new CrearMovimientoRequest("Central Tech", "Estación de trabajo", "Tecnología", 500m, DateTime.UtcNow);

        var response = await client.PostAsJsonAsync($"/api/tarjetas/{tarjetaId}/movimientos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TarjetasDbContext>();
        var movimientos = await dbContext.Movimientos.AsNoTracking().Where(m => m.TarjetaId == tarjetaId).ToListAsync();

        movimientos.Should().ContainSingle(m => m.Comercio == "Central Tech" && m.Importe == 500m);
        var tarjeta = await dbContext.Tarjetas.AsNoTracking().FirstAsync(t => t.Id == tarjetaId);
        tarjeta.SaldoUtilizado.Should().Be(750m);
    }

    private static TarjetaCredito CreateTarjeta(Guid tarjetaId, Guid clienteId, Guid cuentaId, decimal saldoUtilizado)
    {
        return new TarjetaCredito
        {
            Id = tarjetaId,
            ClienteId = clienteId,
            CuentaId = cuentaId,
            NumeroEnmascarado = "4899 **** **** 0021",
            Marca = "Mastercard Black",
            Limite = 1000m,
            SaldoUtilizado = saldoUtilizado,
            PagoMinimo = 100m,
            Cierre = DateTime.UtcNow.AddDays(5),
            Vencimiento = DateTime.UtcNow.AddDays(15)
        };
    }

    private static IEnumerable<MovimientoTarjeta> CreateMovimientos(Guid tarjetaId, Guid clienteId, int count)
    {
        return Enumerable.Range(0, count).Select(index => new MovimientoTarjeta
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjetaId,
            ClienteId = clienteId,
            Comercio = $"Comercio {index}",
            Descripcion = "Cargo automático",
            Categoria = "Servicios",
            Importe = 100m + index,
            Fecha = DateTime.UtcNow.AddDays(-index)
        });
    }

    private static void AddIdentityHeaders(HttpClient client, Guid clienteId, Guid tarjetaId, bool isAdmin)
    {
        client.DefaultRequestHeaders.Remove("x-test-cliente-id");
        client.DefaultRequestHeaders.Remove("x-test-tarjeta-id");
        client.DefaultRequestHeaders.Remove("x-test-is-admin");

        client.DefaultRequestHeaders.Add("x-test-cliente-id", clienteId.ToString());
        client.DefaultRequestHeaders.Add("x-test-tarjeta-id", tarjetaId.ToString());
        client.DefaultRequestHeaders.Add("x-test-is-admin", isAdmin ? "true" : "false");
    }
}
