using System;
using System.Threading;
using System.Threading.Tasks;
using Clientes.Auth;
using Clientes.Contracts;
using Clientes.Data;
using Clientes.Domain;
using Clientes.Security;
using Clientes.Services;
using Clientes.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Clientes.Tests.Services;

public class ClientesServicesTests
{
    [Fact]
    public async Task LoginAsync_ReturnsTokenForValidCredentials()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Ariel Demo",
            Documento = "123",
            Email = "demo@sistema.test",
            PasswordHash = PasswordHasher.Hash("Clave123"),
            CuentaPrincipalId = Guid.NewGuid(),
            TarjetaPrincipalId = Guid.NewGuid(),
            EsAdministrador = false
        };

        await using var context = CreateContext();
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var tokenService = new Mock<IJwtTokenService>();
        tokenService.Setup(s => s.CreateToken(It.IsAny<Cliente>())).Returns("token-demo");
        var options = Options.Create(new JwtOptions { Issuer = "issuer", Audience = "aud", Key = new string('x', 32), ExpirationMinutes = 30 });
        var service = new ClientesService(context, tokenService.Object, options);

        var result = await service.LoginAsync(new LoginRequest(cliente.Email, "Clave123"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Response!.Token.Should().Be("token-demo");
    }

    [Fact]
    public async Task RegisterAsync_ReturnsConflictWhenEmailAlreadyExists()
    {
        await using var context = CreateContext();
        context.Clientes.Add(new Cliente
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Existente",
            Documento = "123",
            Email = "existe@test.com",
            PasswordHash = "hash",
            CuentaPrincipalId = Guid.NewGuid(),
            TarjetaPrincipalId = Guid.NewGuid(),
            EsAdministrador = false
        });
        await context.SaveChangesAsync();

        var tokenService = Mock.Of<IJwtTokenService>();
        var options = Options.Create(new JwtOptions { Issuer = "issuer", Audience = "aud", Key = new string('x', 32), ExpirationMinutes = 30 });
        var service = new ClientesService(context, tokenService, options);

        var result = await service.RegisterAsync(new RegisterRequest("Nombre", "123", "existe@test.com", "123", "Clave123"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorType.Should().Be(OperationErrorType.Conflict);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ReturnsNotFoundWhenClienteMissing()
    {
        await using var context = CreateContext();
        var service = new ClientesService(context, Mock.Of<IJwtTokenService>(), Options.Create(new JwtOptions
        {
            Issuer = "issuer",
            Audience = "aud",
            Key = new string('x', 32),
            ExpirationMinutes = 30
        }));

        var result = await service.UpdatePasswordAsync(new PasswordRecoveryRequest("123", "missing@test.com", "nuevaClave"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorType.Should().Be(OperationErrorType.NotFound);
    }

    [Fact]
    public async Task AdminService_GetOverviewAsync_RequiresAdmin()
    {
        await using var context = CreateContext();
        var userContext = Mock.Of<IUserContext>(_ => _.EsAdministrador == false);
        var adminService = new AdminService(context, userContext);

        var result = await adminService.GetOverviewAsync(CancellationToken.None);
        result.Succeeded.Should().BeFalse();
        result.ErrorType.Should().Be(OperationErrorType.Unauthorized);
    }

    [Fact]
    public async Task AdminService_GetPendingRegistros_ReturnsData()
    {
        await using var context = CreateContext();
        context.Registros.Add(new ClienteRegistro
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Nuevo Cliente",
            Documento = "12345",
            Email = "nuevo@test.com",
            Telefono = "123",
            PasswordHash = "hash",
            Estado = "Pendiente",
            CreadoEnUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var userContext = Mock.Of<IUserContext>(_ => _.EsAdministrador == true);
        var adminService = new AdminService(context, userContext);

        var result = await adminService.GetPendingRegistrosAsync(CancellationToken.None);
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull().And.HaveCount(1);
    }

    private static ClientesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClientesDbContext(options);
    }
}
