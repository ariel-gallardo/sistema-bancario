using System.Security.Claims;
using System.Text;
using Cuentas.Auth;
using Cuentas.Contracts;
using Cuentas.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var sqlConnection = builder.Configuration.GetConnectionString("SqlServer") ??
                    "Server=(localdb)\\MSSQLLocalDB;Database=SistemaBancario.Cuentas;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

builder.Services
    .AddHealthChecks()
    .AddSqlServer(sqlConnection);

builder.Services.AddDbContext<CuentasDbContext>(options => options.UseSqlServer(sqlConnection));
builder.Services.AddScoped<CuentasDbInitializer>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("No se encontró la configuración JWT");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                         ["http://localhost:5173"];

    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.MapHealthChecks("/health");
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

var cuentasApi = app.MapGroup("/api/cuentas").RequireAuthorization().WithTags("Cuentas");

cuentasApi.MapGet("/principal", async (ClaimsPrincipal user, CuentasDbContext db, CancellationToken ct) =>
    {
        var clienteId = EndpointHelpers.GetClienteId(user);
        if (clienteId is null)
        {
            return Results.Unauthorized();
        }

        var principal = await db.Cuentas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.EsPrincipal, ct);

        if (principal is null)
        {
            return Results.NotFound();
        }

        var summary = await EndpointHelpers.BuildAccountSummaryAsync(clienteId.Value, principal.Id, db, ct);
        return summary is null ? Results.NotFound() : Results.Ok(summary);
    })
    .WithName("GetCuentaPrincipal");

cuentasApi.MapGet("/{cuentaId:guid}", async (Guid cuentaId, ClaimsPrincipal user, CuentasDbContext db, CancellationToken ct) =>
    {
        var clienteId = EndpointHelpers.GetClienteId(user);
        if (clienteId is null)
        {
            return Results.Unauthorized();
        }

        var summary = await EndpointHelpers.BuildAccountSummaryAsync(clienteId.Value, cuentaId, db, ct);
        return summary is null ? Results.NotFound() : Results.Ok(summary);
    })
    .WithName("GetCuentaPorId");

cuentasApi.MapPut("/{cuentaId:guid}/favorita", async (Guid cuentaId, ClaimsPrincipal user, CuentasDbContext db, CancellationToken ct) =>
    {
        var clienteId = EndpointHelpers.GetClienteId(user);
        if (clienteId is null)
        {
            return Results.Unauthorized();
        }

        var cuentasCliente = await db.Cuentas
            .Where(c => c.ClienteId == clienteId)
            .ToListAsync(ct);

        if (!cuentasCliente.Any(c => c.Id == cuentaId))
        {
            return Results.NotFound();
        }

        foreach (var cuenta in cuentasCliente)
        {
            cuenta.EsFavorita = cuenta.Id == cuentaId;
        }

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    })
    .WithName("SetCuentaFavorita");

cuentasApi.MapDelete("/favorita", async (ClaimsPrincipal user, CuentasDbContext db, CancellationToken ct) =>
    {
        var clienteId = EndpointHelpers.GetClienteId(user);
        if (clienteId is null)
        {
            return Results.Unauthorized();
        }

        var favoritas = await db.Cuentas
            .Where(c => c.ClienteId == clienteId && c.EsFavorita)
            .ToListAsync(ct);

        if (favoritas.Count == 0)
        {
            return Results.NoContent();
        }

        foreach (var cuenta in favoritas)
        {
            cuenta.EsFavorita = false;
        }

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    })
    .WithName("ClearCuentaFavorita");

app.MapGet("/", () => "Servicio de Cuentas listo");

await app.SeedCuentasAsync();

app.Run();

static class EndpointHelpers
{
    public static Guid? GetClienteId(ClaimsPrincipal user)
    {
        var clienteIdValue = user.FindFirstValue("clienteId");
        return Guid.TryParse(clienteIdValue, out var clienteId) ? clienteId : null;
    }

    public static async Task<AccountSummaryResponse?> BuildAccountSummaryAsync(
        Guid clienteId,
        Guid cuentaId,
        CuentasDbContext db,
        CancellationToken ct)
    {
        var cuenta = await db.Cuentas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Id == cuentaId, ct);

        if (cuenta is null)
        {
            return null;
        }

        var otrasCuentas = await db.Cuentas.AsNoTracking()
            .Where(c => c.ClienteId == clienteId && c.Id != cuenta.Id)
            .OrderByDescending(c => c.EsPrincipal)
            .Select(c => new AccountSnapshot(
                c.Id,
                c.Alias,
                c.Moneda,
                c.SaldoActual,
                c.EsPrincipal,
                c.EsFavorita))
            .ToListAsync(ct);

        var saldoDisponible = cuenta.SaldoActual + cuenta.LimiteDescubierto;

        return new AccountSummaryResponse(
            cuenta.Id,
            cuenta.Alias,
            cuenta.Banco,
            cuenta.Numero,
            cuenta.Moneda,
            cuenta.SaldoActual,
            saldoDisponible,
            cuenta.UltimaActualizacion,
            cuenta.EsPrincipal,
            cuenta.EsFavorita,
            otrasCuentas);
    }
}
