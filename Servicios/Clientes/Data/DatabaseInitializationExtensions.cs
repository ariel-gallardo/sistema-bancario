using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Clientes.Data;

public static class DatabaseInitializationExtensions
{
    public static async Task SeedClientesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ClientesDbInitializer>();
        await initializer.SeedAsync();
    }
}
