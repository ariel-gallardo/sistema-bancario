using System.Text;
using Clientes.Auth;
using Clientes.Contracts;
using Clientes.Data;
using Clientes.Infrastructure;
using Clientes.Security;
using Clientes.Services;
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
builder.Services.AddScoped<IClientesService, ClientesService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

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
        IClientesService clientesService,
        CancellationToken ct) =>
    {
        var result = await clientesService.LoginAsync(request, ct);

        if (result.Succeeded && result.Response is not null)
        {
            return Results.Ok(result.Response);
        }

        return result.ErrorType switch
        {
            LoginErrorType.InvalidInput => Results.BadRequest(new { message = result.ErrorMessage ?? "Solicitud inválida" }),
            LoginErrorType.Unauthorized => Results.Unauthorized(),
            _ => Results.Problem(result.ErrorMessage ?? "No pudimos iniciar sesión"),
        };
    })
    .WithTags("Auth");

api.MapPost("/auth/register", async (
        RegisterRequest request,
        IClientesService clientesService,
        CancellationToken ct) =>
    {
        var result = await clientesService.RegisterAsync(request, ct);

        if (result.Succeeded && result.Data is not null)
        {
            return Results.Created($"/api/auth/register/{result.Data.RegistroId}", result.Data);
        }

        return result.ErrorType switch
        {
            OperationErrorType.InvalidInput => Results.BadRequest(new { message = result.ErrorMessage ?? "Solicitud inválida" }),
            OperationErrorType.Conflict => Results.Conflict(new { message = result.ErrorMessage ?? "La solicitud no pudo completarse" }),
            _ => Results.Problem(result.ErrorMessage ?? "No pudimos registrar la solicitud"),
        };
    })
    .WithTags("Auth");

api.MapPost("/auth/password", async (
        PasswordRecoveryRequest request,
        IClientesService clientesService,
        CancellationToken ct) =>
    {
        var result = await clientesService.UpdatePasswordAsync(request, ct);

        if (result.Succeeded)
        {
            return Results.Ok(new PasswordRecoveryResponse("Actualizamos tu clave. Podés volver a iniciar sesión."));
        }

        return result.ErrorType switch
        {
            OperationErrorType.InvalidInput => Results.BadRequest(new { message = result.ErrorMessage ?? "Solicitud inválida" }),
            OperationErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage ?? "No encontramos coincidencias" }),
            _ => Results.Problem(result.ErrorMessage ?? "No pudimos actualizar la clave"),
        };
    })
    .WithTags("Auth");

api.MapGet("/clientes/me", async (
        IUserContext userContext,
        IClientesService clientesService,
        CancellationToken ct) =>
    {
        if (userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var profile = await clientesService.GetProfileAsync(userContext.ClienteId.Value, ct);
        return profile is null ? Results.NotFound() : Results.Ok(profile);
    })
    .RequireAuthorization()
    .WithTags("Clientes");

var adminApi = api.MapGroup("/admin")
    .RequireAuthorization()
    .WithTags("Admin");

adminApi.MapGet("/overview", async (IAdminService adminService, CancellationToken ct) =>
    {
        var result = await adminService.GetOverviewAsync(ct);
        if (result.Succeeded && result.Data is not null)
        {
            return Results.Ok(result.Data);
        }

        return result.ErrorType switch
        {
            OperationErrorType.Unauthorized => Results.Forbid(),
            _ => Results.Problem(result.ErrorMessage ?? "No pudimos obtener el resumen"),
        };
    });

adminApi.MapGet("/registros/pendientes", async (IAdminService adminService, CancellationToken ct) =>
    {
        var result = await adminService.GetPendingRegistrosAsync(ct);
        if (result.Succeeded && result.Data is not null)
        {
            return Results.Ok(result.Data);
        }

        return result.ErrorType switch
        {
            OperationErrorType.Unauthorized => Results.Forbid(),
            _ => Results.Problem(result.ErrorMessage ?? "No pudimos obtener las solicitudes"),
        };
    });

adminApi.MapPost("/registros/{registroId:guid}/revisar", async (Guid registroId, IAdminService adminService, CancellationToken ct) =>
    {
        var result = await adminService.MarkRegistroAsReviewedAsync(registroId, ct);
        if (result.Succeeded)
        {
            return Results.NoContent();
        }

        return result.ErrorType switch
        {
            OperationErrorType.Unauthorized => Results.Forbid(),
            OperationErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage ?? "No encontramos la solicitud" }),
            _ => Results.Problem(result.ErrorMessage ?? "No pudimos actualizar la solicitud"),
        };
    });

app.MapGet("/", () => "Servicio de Clientes listo");

await app.SeedClientesAsync();

app.Run();
