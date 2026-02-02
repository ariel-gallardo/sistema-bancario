using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pagos.Contracts;
using Pagos.Data;
using Pagos.Domain;
using Pagos.Infrastructure;
using Pagos.Services;
using Xunit;

namespace Pagos.Tests.Services;

public class PagosServiceTests
{
    [Fact]
    public async Task GetPagosProgramadosAsync_ReturnsUnauthorizedWhenMissingCliente()
    {
        await using var context = CreateContext();
        var userContext = Mock.Of<IUserContext>(_ => _.ClienteId == null);
        var service = new PagosService(context, userContext);

        var result = await service.GetPagosProgramadosAsync(CancellationToken.None);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, status.StatusCode);
    }

    [Fact]
    public async Task GetPagosHistorialAsync_ReturnsDescendingList()
    {
        var clienteId = Guid.NewGuid();
        await using var context = CreateContext();
        context.PagosHistoricos.AddRange(CreateHistorial(clienteId));
        await context.SaveChangesAsync();

        var userContext = Mock.Of<IUserContext>(_ => _.ClienteId == clienteId);
        var service = new PagosService(context, userContext);

        var result = await service.GetPagosHistorialAsync(take: 3, CancellationToken.None);
        var okResult = Assert.IsType<Ok<PagosHistorialResponse>>(result);
        var response = Assert.IsType<PagosHistorialResponse>(okResult.Value);
        response.Historial.Should().HaveCount(3);
        response.Historial.Should().BeInDescendingOrder(p => p.FechaPago);
    }

    private static PagosDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PagosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PagosDbContext(options);
    }

    private static IEnumerable<PagoHistorico> CreateHistorial(Guid clienteId)
    {
        return Enumerable.Range(0, 5).Select(index => new PagoHistorico
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Empresa = $"Empresa {index}",
            Categoria = "Servicios",
            Importe = 100m + index,
            Moneda = "ARS",
            FechaPago = DateTime.UtcNow.AddDays(-index),
            MedioPago = "Cuenta sueldos",
            FueDebitoAutomatico = index % 2 == 0
        });
    }
}
