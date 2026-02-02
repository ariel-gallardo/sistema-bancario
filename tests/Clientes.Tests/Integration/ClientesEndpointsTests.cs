using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Clientes.Contracts;
using Clientes.Data;
using Clientes.Domain;
using Clientes.Security;
using Clientes.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Clientes.Tests.Integration;

public class ClientesEndpointsTests : IClassFixture<ClientesWebApplicationFactory>
{
    private readonly ClientesWebApplicationFactory _factory;

    public ClientesEndpointsTests(ClientesWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginEndpoint_ReturnsTokenForValidCredentials()
    {
        var password = "Clave123";
        await _factory.WithDataAsync(context =>
        {
            context.Clientes.Add(new Cliente
            {
                Id = Guid.NewGuid(),
                NombreCompleto = "Test",
                Documento = "123",
                Email = "login@test.com",
                PasswordHash = PasswordHasher.Hash(password),
                CuentaPrincipalId = Guid.NewGuid(),
                TarjetaPrincipalId = Guid.NewGuid(),
                EsAdministrador = false
            });
            return Task.CompletedTask;
        });

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("login@test.com", password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();
        payload!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetProfile_ReturnsClienteData()
    {
        var clienteId = Guid.NewGuid();
        await _factory.WithDataAsync(context =>
        {
            context.Clientes.Add(new Cliente
            {
                Id = clienteId,
                NombreCompleto = "Perfil",
                Documento = "321",
                Email = "perfil@test.com",
                PasswordHash = "hash",
                CuentaPrincipalId = Guid.NewGuid(),
                TarjetaPrincipalId = Guid.NewGuid(),
                EsAdministrador = false
            });
            return Task.CompletedTask;
        });

        var client = _factory.CreateClient();
        AddIdentityHeaders(client, clienteId, isAdmin: false);

        var response = await client.GetAsync("/api/clientes/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ClienteProfileResponse>();
        profile.Should().NotBeNull();
        profile!.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public async Task AdminOverview_RequiresAdminClaim()
    {
        await _factory.WithDataAsync(context => Task.CompletedTask);
        var client = _factory.CreateClient();
        AddIdentityHeaders(client, Guid.NewGuid(), isAdmin: true);

        var response = await client.GetAsync("/api/admin/overview");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var overview = await response.Content.ReadFromJsonAsync<AdminOverviewResponse>();
        overview.Should().NotBeNull();
    }

    private static void AddIdentityHeaders(HttpClient client, Guid clienteId, bool isAdmin)
    {
        client.DefaultRequestHeaders.Remove("x-test-cliente-id");
        client.DefaultRequestHeaders.Remove("x-test-is-admin");
        client.DefaultRequestHeaders.Add("x-test-cliente-id", clienteId.ToString());
        client.DefaultRequestHeaders.Add("x-test-is-admin", isAdmin ? "true" : "false");
    }
}
