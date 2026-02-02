using System.Text;
using Cuentas.Auth;
using Cuentas.Data;
using Cuentas.Infrastructure;
using Cuentas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddScoped<ICuentasService, CuentasService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

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

cuentasApi.MapGet("/principal", async (IUserContext userContext, ICuentasService cuentasService, CancellationToken ct) =>
    {
        if (userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var summary = await cuentasService.GetCuentaPrincipalAsync(userContext.ClienteId.Value, ct);
        return summary is null ? Results.NotFound() : Results.Ok(summary);
    })
    .WithName("GetCuentaPrincipal");

cuentasApi.MapGet("/{cuentaId:guid}", async (Guid cuentaId, IUserContext userContext, ICuentasService cuentasService, CancellationToken ct) =>
    {
        if (userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var summary = await cuentasService.GetCuentaByIdAsync(userContext.ClienteId.Value, cuentaId, ct);
        return summary is null ? Results.NotFound() : Results.Ok(summary);
    })
    .WithName("GetCuentaPorId");

cuentasApi.MapPut("/{cuentaId:guid}/favorita", async (Guid cuentaId, IUserContext userContext, ICuentasService cuentasService, CancellationToken ct) =>
    {
        if (userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var updated = await cuentasService.SetCuentaFavoritaAsync(userContext.ClienteId.Value, cuentaId, ct);
        return updated ? Results.NoContent() : Results.NotFound();
    })
    .WithName("SetCuentaFavorita");

cuentasApi.MapDelete("/favorita", async (IUserContext userContext, ICuentasService cuentasService, CancellationToken ct) =>
    {
        if (userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        await cuentasService.ClearCuentaFavoritaAsync(userContext.ClienteId.Value, ct);
        return Results.NoContent();
    })
    .WithName("ClearCuentaFavorita");

app.MapGet("/", () => "Servicio de Cuentas listo");

await app.SeedCuentasAsync();

app.Run();
