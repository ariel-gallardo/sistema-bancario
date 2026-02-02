using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Cuentas.Contracts;
using Cuentas.Data;
using Cuentas.Domain;
using Cuentas.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cuentas.Tests.Integration;

public class CuentasEndpointsTests : IClassFixture<CuentasWebApplicationFactory>
{
    private readonly CuentasWebApplicationFactory _factory;

    public CuentasEndpointsTests(CuentasWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCuentaPrincipal_ReturnsSummaryForCliente()
    {
        var clienteId = Guid.NewGuid();
        var principalId = Guid.NewGuid();
        await _factory.WithDataAsync(context =>
        {
            context.Cuentas.Add(CreateCuenta(principalId, clienteId, principal: true));
            context.Cuentas.Add(CreateCuenta(Guid.NewGuid(), clienteId, alias: "Caja", principal: false));
            return Task.CompletedTask;
        });

        var client = _factory.CreateClient();
        AddIdentityHeaders(client, clienteId);

        var response = await client.GetAsync("/api/cuentas/principal");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AccountSummaryResponse>();
        payload.Should().NotBeNull();
        payload!.CuentaId.Should().Be(principalId);
        payload.OtrasCuentas.Should().HaveCount(1);
    }

    [Fact]
    public async Task SetCuentaFavorita_PersistsChange()
    {
        var clienteId = Guid.NewGuid();
        var targetAccount = Guid.NewGuid();
        await _factory.WithDataAsync(context =>
        {
            context.Cuentas.Add(CreateCuenta(targetAccount, clienteId, esFavorita: false));
            context.Cuentas.Add(CreateCuenta(Guid.NewGuid(), clienteId, esFavorita: true));
            return Task.CompletedTask;
        });

        var client = _factory.CreateClient();
        AddIdentityHeaders(client, clienteId);

        var response = await client.PutAsync($"/api/cuentas/{targetAccount}/favorita", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CuentasDbContext>();
        var cuentas = await dbContext.Cuentas.AsNoTracking().Where(c => c.ClienteId == clienteId).ToListAsync();
        cuentas.Should().ContainSingle(c => c.Id == targetAccount && c.EsFavorita);
    }

    private static CuentaBancaria CreateCuenta(Guid cuentaId, Guid clienteId, bool principal = false, bool esFavorita = false, string alias = "Cuenta")
    {
        return new CuentaBancaria
        {
            Id = cuentaId,
            ClienteId = clienteId,
            Alias = alias,
            Banco = "Banco Demo",
            Numero = "00-123",
            Moneda = "ARS",
            SaldoActual = 1000m,
            LimiteDescubierto = 10000m,
            EsPrincipal = principal,
            EsFavorita = esFavorita,
            UltimaActualizacion = DateTime.UtcNow
        };
    }

    private static void AddIdentityHeaders(HttpClient client, Guid clienteId)
    {
        client.DefaultRequestHeaders.Remove("x-test-cliente-id");
        client.DefaultRequestHeaders.Add("x-test-cliente-id", clienteId.ToString());
    }
}
