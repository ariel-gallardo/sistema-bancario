using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pagos.Auth;
using Pagos.Contracts;
using Pagos.Data;
using Pagos.Infrastructure;
using Pagos.Services;

var builder = WebApplication.CreateBuilder(args);

var sqlConnection = builder.Configuration.GetConnectionString("SqlServer") ??
                    "Server=(localdb)\\MSSQLLocalDB;Database=SistemaBancario.Pagos;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Services
    .AddHealthChecks()
    .AddSqlServer(sqlConnection);

if (isTesting)
{
    var testingDatabaseName = builder.Configuration.GetValue<string>("Testing:DatabaseName") ?? "PagosTesting";
    builder.Services.AddDbContext<PagosDbContext>(options => options.UseInMemoryDatabase(testingDatabaseName));
}
else
{
    builder.Services.AddDbContext<PagosDbContext>(options => options.UseSqlServer(sqlConnection));
}
builder.Services.AddScoped<PagosDbInitializer>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IPagosService, PagosService>();

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

pagosApi.MapGet("/programados",
        (IPagosService service, CancellationToken ct) => service.GetPagosProgramadosAsync(ct))
    .WithName("GetPagosProgramados");

pagosApi.MapGet("/historial",
        (int? take, IPagosService service, CancellationToken ct) => service.GetPagosHistorialAsync(take, ct))
    .WithName("GetPagosHistorial");

app.MapGet("/", () => "Servicio de Pagos listo");

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.SeedPagosAsync();
}

app.Run();

public partial class Program;
