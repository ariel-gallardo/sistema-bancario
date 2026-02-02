using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Pagos.Data;

public static class DatabaseInitializationExtensions
{
    public static async Task SeedPagosAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<PagosDbInitializer>();
        await initializer.SeedAsync();
    }
}
