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
        var clienteIdValue = user.FindFirstValue("clienteId");
        if (!Guid.TryParse(clienteIdValue, out var clienteId))
        {
            return Results.Unauthorized();
        }

        var principal = await db.Cuentas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.EsPrincipal, ct);

        if (principal is null)
        {
            return Results.NotFound();
        }

        var otrasCuentas = await db.Cuentas.AsNoTracking()
            .Where(c => c.ClienteId == clienteId && c.Id != principal.Id)
            .OrderByDescending(c => c.EsPrincipal)
            .Select(c => new AccountSnapshot(
                c.Id,
                c.Alias,
                c.Moneda,
                c.SaldoActual,
                c.EsPrincipal))
            .ToListAsync(ct);

        var saldoDisponible = principal.SaldoActual + principal.LimiteDescubierto;

        return Results.Ok(new AccountSummaryResponse(
            principal.Id,
            principal.Alias,
            principal.Banco,
            principal.Numero,
            principal.Moneda,
            principal.SaldoActual,
            saldoDisponible,
            principal.UltimaActualizacion,
            otrasCuentas));
    })
    .WithName("GetCuentaPrincipal");

app.MapGet("/", () => "Servicio de Cuentas listo");

await app.SeedCuentasAsync();

app.Run();
