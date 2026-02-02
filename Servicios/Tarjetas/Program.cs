using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tarjetas.Auth;
using Tarjetas.Contracts;
using Tarjetas.Data;
using Tarjetas.Infrastructure;
using Tarjetas.Services;

var builder = WebApplication.CreateBuilder(args);

var sqlConnection = builder.Configuration.GetConnectionString("SqlServer") ??
                    "Server=(localdb)\\MSSQLLocalDB;Database=SistemaBancario.Tarjetas;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Services
    .AddHealthChecks()
    .AddSqlServer(sqlConnection);

if (isTesting)
{
    var testingDatabaseName = builder.Configuration.GetValue<string>("Testing:DatabaseName") ?? "TarjetasTesting";
    builder.Services.AddDbContext<TarjetasDbContext>(options => options.UseInMemoryDatabase(testingDatabaseName));
}
else
{
    builder.Services.AddDbContext<TarjetasDbContext>(options => options.UseSqlServer(sqlConnection));
}
builder.Services.AddScoped<TarjetasDbInitializer>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<ITarjetasService, TarjetasService>();

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

tarjetasApi.MapGet("/principal/movimientos",
        (int? take, ITarjetasService service, CancellationToken ct) =>
            service.GetPrincipalMovimientosAsync(take, ct))
    .WithName("GetMovimientosTarjetaPrincipal");

tarjetasApi.MapGet("/cuentas/{cuentaId:guid}",
    (Guid cuentaId, ITarjetasService service, CancellationToken ct) =>
        service.GetTarjetasByCuentaAsync(cuentaId, ct))
    .WithName("GetTarjetasByCuenta");

tarjetasApi.MapGet("/{tarjetaId:guid}/movimientos",
    (Guid tarjetaId, int? take, ITarjetasService service, CancellationToken ct) =>
        service.GetMovimientosByTarjetaAsync(tarjetaId, take, ct))
    .WithName("GetMovimientosPorTarjeta");

tarjetasApi.MapPost("/{tarjetaId:guid}/movimientos",
        (Guid tarjetaId, CrearMovimientoRequest request, ITarjetasService service, CancellationToken ct) =>
            service.AddMovimientoAsync(tarjetaId, request, ct))
    .WithName("AddMovimientoTarjeta");

app.MapGet("/", () => "Servicio de Tarjetas listo");

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.SeedTarjetasAsync();
}

app.Run();

public partial class Program;
