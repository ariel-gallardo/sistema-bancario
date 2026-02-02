using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cuentas.Data;
using Cuentas.Domain;
using Cuentas.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Cuentas.Tests.Services;

public class CuentasServiceTests
{
    [Fact]
    public async Task GetCuentaPrincipalAsync_ReturnsSummaryIncludingOtrasCuentas()
    {
        var clienteId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Cuentas.AddRange(
            CreateCuenta(clienteId, principal: true, alias: "Principal"),
            CreateCuenta(clienteId, principal: false, alias: "Caja de ahorro"));
        await context.SaveChangesAsync();

        var service = new CuentasService(context);

        var summary = await service.GetCuentaPrincipalAsync(clienteId, CancellationToken.None);

        summary.Should().NotBeNull();
        summary!.Alias.Should().Be("Principal");
        summary.OtrasCuentas.Should().ContainSingle(snapshot => snapshot.Alias == "Caja de ahorro");
        summary.SaldoDisponible.Should().Be(summary.SaldoActual + 10000m);
    }

    [Fact]
    public async Task SetCuentaFavoritaAsync_TogglesOnlySelectedCuenta()
    {
        var clienteId = Guid.NewGuid();
        var cuentaId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Cuentas.AddRange(
            CreateCuenta(clienteId, id: cuentaId, esFavorita: false, alias: "Cuenta sueldo"),
            CreateCuenta(clienteId, esFavorita: true, alias: "Ahorro"));
        await context.SaveChangesAsync();

        var service = new CuentasService(context);

        var updated = await service.SetCuentaFavoritaAsync(clienteId, cuentaId, CancellationToken.None);

        updated.Should().BeTrue();
        var cuentas = await context.Cuentas.AsNoTracking().Where(c => c.ClienteId == clienteId).ToListAsync();
        cuentas.Should().ContainSingle(c => c.Id == cuentaId && c.EsFavorita);
        cuentas.Should().ContainSingle(c => c.Alias == "Ahorro" && !c.EsFavorita);
    }

    [Fact]
    public async Task ClearCuentaFavoritaAsync_ReturnsFalseWhenNoFavorites()
    {
        var clienteId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Cuentas.Add(CreateCuenta(clienteId, esFavorita: false));
        await context.SaveChangesAsync();

        var service = new CuentasService(context);

        var result = await service.ClearCuentaFavoritaAsync(clienteId, CancellationToken.None);

        result.Should().BeFalse();
    }

    private static CuentasDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CuentasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CuentasDbContext(options);
    }

    private static CuentaBancaria CreateCuenta(
        Guid clienteId,
        Guid? id = null,
        bool principal = false,
        bool esFavorita = false,
        string alias = "Cuenta",
        decimal saldo = 5000m)
    {
        return new CuentaBancaria
        {
            Id = id ?? Guid.NewGuid(),
            ClienteId = clienteId,
            Alias = alias,
            Banco = "Banco Demo",
            Numero = "00112233",
            Moneda = "ARS",
            SaldoActual = saldo,
            LimiteDescubierto = 10000m,
            EsPrincipal = principal,
            EsFavorita = esFavorita,
            UltimaActualizacion = DateTime.UtcNow
        };
    }
}
