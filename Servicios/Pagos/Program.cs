using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pagos.Auth;
using Pagos.Contracts;
using Pagos.Data;

var builder = WebApplication.CreateBuilder(args);

var sqlConnection = builder.Configuration.GetConnectionString("SqlServer") ??
                    "Server=(localdb)\\MSSQLLocalDB;Database=SistemaBancario.Pagos;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

builder.Services
    .AddHealthChecks()
    .AddSqlServer(sqlConnection);

builder.Services.AddDbContext<PagosDbContext>(options => options.UseSqlServer(sqlConnection));
builder.Services.AddScoped<PagosDbInitializer>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("No se encontró la configuración JWT para Pagos");

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

var pagosApi = app.MapGroup("/api/pagos")
    .RequireAuthorization()
    .WithTags("Pagos");

pagosApi.MapGet("/programados", async (ClaimsPrincipal user, PagosDbContext db, CancellationToken ct) =>
    {
        var clienteIdValue = user.FindFirstValue("clienteId");
        if (!Guid.TryParse(clienteIdValue, out var clienteId))
        {
            return Results.Unauthorized();
        }

        var programados = await db.PagosProgramados.AsNoTracking()
            .Where(p => p.ClienteId == clienteId)
            .OrderBy(p => p.FechaProgramada)
            .Select(p => new PagoProgramadoResponse(
                p.Id,
                p.Empresa,
                p.Descripcion,
                p.Importe,
                p.Moneda,
                p.FechaProgramada,
                p.EsDebitoAutomatico,
                p.Estado))
            .ToListAsync(ct);

        return Results.Ok(new PagosProgramadosResponse(programados));
    })
    .WithName("GetPagosProgramados");

pagosApi.MapGet("/historial", async (ClaimsPrincipal user, PagosDbContext db, int? take, CancellationToken ct) =>
    {
        var clienteIdValue = user.FindFirstValue("clienteId");
        if (!Guid.TryParse(clienteIdValue, out var clienteId))
        {
            return Results.Unauthorized();
        }

        var limit = Math.Clamp(take.GetValueOrDefault(5), 1, 20);

        var historial = await db.PagosHistoricos.AsNoTracking()
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaPago)
            .Take(limit)
            .Select(p => new PagoHistoricoResponse(
                p.Id,
                p.Empresa,
                p.Categoria,
                p.Importe,
                p.Moneda,
                p.FechaPago,
                p.MedioPago,
                p.FueDebitoAutomatico))
            .ToListAsync(ct);

        return Results.Ok(new PagosHistorialResponse(historial));
    })
    .WithName("GetPagosHistorial");

app.MapGet("/", () => "Servicio de Pagos listo");

await app.SeedPagosAsync();

app.Run();
