using System.Security.Claims;
using System.Text;
using Clientes.Auth;
using Clientes.Contracts;
using Clientes.Data;
using Clientes.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var sqlConnection = builder.Configuration.GetConnectionString("SqlServer") ??
                    "Server=(localdb)\\MSSQLLocalDB;Database=SistemaBancario.Clientes;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

builder.Services
    .AddHealthChecks()
    .AddSqlServer(sqlConnection);

builder.Services.AddDbContext<ClientesDbContext>(options =>
    options.UseSqlServer(sqlConnection));

builder.Services.AddScoped<ClientesDbInitializer>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("No se encontró la sección Jwt en la configuración");

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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

var api = app.MapGroup("/api");

api.MapPost("/auth/login", async (
        LoginRequest request,
        ClientesDbContext db,
        IJwtTokenService tokenService,
        IOptions<JwtOptions> jwtOptions,
        CancellationToken ct) =>
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Debes ingresar email y contraseña" });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == normalizedEmail, ct);

        if (cliente is null || !PasswordHasher.Verify(request.Password, cliente.PasswordHash))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(150), ct); // evita ataques de fuerza bruta simples
            return Results.Unauthorized();
        }

        var token = tokenService.CreateToken(cliente);
        var expiresUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);

        return Results.Ok(new LoginResponse(
            cliente.Id,
            cliente.CuentaPrincipalId,
            cliente.TarjetaPrincipalId,
            cliente.NombreCompleto,
            cliente.Email,
            token,
            expiresUtc));
    })
    .WithTags("Auth");

api.MapGet("/clientes/me", async (
        ClaimsPrincipal user,
        ClientesDbContext db,
        CancellationToken ct) =>
    {
        var clienteIdValue = user.FindFirstValue("clienteId");
        if (!Guid.TryParse(clienteIdValue, out var clienteId))
        {
            return Results.Unauthorized();
        }

        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clienteId, ct);

        if (cliente is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new ClienteProfileResponse(
            cliente.Id,
            cliente.NombreCompleto,
            cliente.Email,
            cliente.Documento,
            cliente.CuentaPrincipalId,
            cliente.TarjetaPrincipalId));
    })
    .RequireAuthorization()
    .WithTags("Clientes");

app.MapGet("/", () => "Servicio de Clientes listo");

await app.SeedClientesAsync();

app.Run();
