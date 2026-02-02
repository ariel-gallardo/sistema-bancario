using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tarjetas.Auth;
using Tarjetas.Contracts;
using Tarjetas.Data;

var builder = WebApplication.CreateBuilder(args);

var sqlConnection = builder.Configuration.GetConnectionString("SqlServer") ??
                    "Server=(localdb)\\MSSQLLocalDB;Database=SistemaBancario.Tarjetas;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

builder.Services
    .AddHealthChecks()
    .AddSqlServer(sqlConnection);

builder.Services.AddDbContext<TarjetasDbContext>(options => options.UseSqlServer(sqlConnection));
builder.Services.AddScoped<TarjetasDbInitializer>();

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

var tarjetasApi = app.MapGroup("/api/tarjetas").RequireAuthorization().WithTags("Tarjetas");

tarjetasApi.MapGet("/principal/movimientos", async (
        ClaimsPrincipal user,
        TarjetasDbContext db,
        int? take,
        CancellationToken ct) =>
    {
        var clienteIdValue = user.FindFirstValue("clienteId");
        if (!Guid.TryParse(clienteIdValue, out var clienteId))
        {
            return Results.Unauthorized();
        }

        var tarjeta = await db.Tarjetas.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ClienteId == clienteId, ct);

        if (tarjeta is null)
        {
            return Results.NotFound();
        }

        var limit = Math.Clamp(take.GetValueOrDefault(5), 1, 20);

        var movimientos = await db.Movimientos.AsNoTracking()
            .Where(m => m.TarjetaId == tarjeta.Id)
            .OrderByDescending(m => m.Fecha)
            .Take(limit)
            .Select(m => new MovimientoTarjetaResponse(
                m.Id,
                m.Comercio,
                m.Descripcion,
                m.Categoria,
                m.Importe,
                m.Fecha))
            .ToListAsync(ct);

        var disponible = Math.Max(0, tarjeta.Limite - tarjeta.SaldoUtilizado);

        return Results.Ok(new TarjetaMovimientosResponse(
            tarjeta.Id,
            tarjeta.Marca,
            tarjeta.NumeroEnmascarado,
            tarjeta.Limite,
            tarjeta.SaldoUtilizado,
            disponible,
            tarjeta.PagoMinimo,
            tarjeta.Cierre,
            tarjeta.Vencimiento,
            movimientos));
    })
    .WithName("GetMovimientosTarjetaPrincipal");

app.MapGet("/", () => "Servicio de Tarjetas listo");

await app.SeedTarjetasAsync();

app.Run();
