using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Tarjetas.Data;

public static class DatabaseInitializationExtensions
{
    public static async Task SeedTarjetasAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<TarjetasDbInitializer>();
        await initializer.SeedAsync();
    }
}
