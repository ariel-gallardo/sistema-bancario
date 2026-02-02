using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Pagos.Contracts;
using Pagos.Data;
using Pagos.Domain;
using Pagos.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Pagos.Tests.Integration;

public class PagosEndpointsTests : IClassFixture<PagosWebApplicationFactory>
{
    private readonly PagosWebApplicationFactory _factory;

    public PagosEndpointsTests(PagosWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProgramados_ReturnsEntries()
    {
        var clienteId = Guid.NewGuid();
        await _factory.WithDataAsync(context =>
        {
            context.PagosProgramados.AddRange(CreateProgramados(clienteId));
            return Task.CompletedTask;
        });

        var client = _factory.CreateClient();
        AddIdentityHeaders(client, clienteId);

        var response = await client.GetAsync("/api/pagos/programados");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<PagosProgramadosResponse>();
        payload.Should().NotBeNull();
        payload!.Programados.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHistorial_RespectsTakeParameter()
    {
        var clienteId = Guid.NewGuid();
        await _factory.WithDataAsync(context =>
        {
            context.PagosHistoricos.AddRange(CreateHistorial(clienteId));
            return Task.CompletedTask;
        });

        var client = _factory.CreateClient();
        AddIdentityHeaders(client, clienteId);

        var response = await client.GetAsync("/api/pagos/historial?take=3");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<PagosHistorialResponse>();
        payload.Should().NotBeNull();
        payload!.Historial.Should().HaveCount(3);
    }

    private static IEnumerable<PagoProgramado> CreateProgramados(Guid clienteId)
    {
        return Enumerable.Range(0, 2).Select(index => new PagoProgramado
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Empresa = $"Empresa {index}",
            Descripcion = "Servicio",
            Importe = 5000m,
            Moneda = "ARS",
            FechaProgramada = DateTime.UtcNow.AddDays(index),
            EsDebitoAutomatico = index % 2 == 0,
            Estado = "Pendiente"
        });
    }

    private static IEnumerable<PagoHistorico> CreateHistorial(Guid clienteId)
    {
        return Enumerable.Range(0, 5).Select(index => new PagoHistorico
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Empresa = $"Empresa {index}",
            Categoria = "Servicios",
            Importe = 4000m + index,
            Moneda = "ARS",
            FechaPago = DateTime.UtcNow.AddDays(-index),
            MedioPago = "Visa",
            FueDebitoAutomatico = index % 2 == 0
        });
    }

    private static void AddIdentityHeaders(HttpClient client, Guid clienteId)
    {
        client.DefaultRequestHeaders.Remove("x-test-cliente-id");
        client.DefaultRequestHeaders.Add("x-test-cliente-id", clienteId.ToString());
    }
}
