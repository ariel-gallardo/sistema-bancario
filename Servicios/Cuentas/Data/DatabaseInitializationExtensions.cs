using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cuentas.Data;

public static class DatabaseInitializationExtensions
{
    public static async Task SeedCuentasAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<CuentasDbInitializer>();
        await initializer.SeedAsync();
    }
}
