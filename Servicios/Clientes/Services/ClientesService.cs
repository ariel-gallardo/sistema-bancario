using Clientes.Auth;
using Clientes.Contracts;
using Clientes.Data;
using Clientes.Domain;
using Clientes.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clientes.Services;

public class ClientesService(
    ClientesDbContext dbContext,
    IJwtTokenService tokenService,
    IOptions<JwtOptions> jwtOptions) : IClientesService
{
    private readonly ClientesDbContext _dbContext = dbContext;
    private readonly IJwtTokenService _tokenService = tokenService;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginResult.InvalidInput("Debes ingresar email y contraseña");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var cliente = await _dbContext.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == normalizedEmail, cancellationToken);

        if (cliente is null || !PasswordHasher.Verify(request.Password, cliente.PasswordHash))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
            return LoginResult.Unauthorized();
        }

        var token = _tokenService.CreateToken(cliente);
        var expiresUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        var response = new LoginResponse(
            cliente.Id,
            cliente.CuentaPrincipalId,
            cliente.TarjetaPrincipalId,
            cliente.NombreCompleto,
            cliente.Email,
            cliente.EsAdministrador,
            token,
            expiresUtc);

        return LoginResult.Success(response);
    }

    public async Task<ClienteProfileResponse?> GetProfileAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _dbContext.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clienteId, cancellationToken);

        if (cliente is null)
        {
            return null;
        }

        return new ClienteProfileResponse(
            cliente.Id,
            cliente.NombreCompleto,
            cliente.Email,
            cliente.Documento,
            cliente.CuentaPrincipalId,
            cliente.TarjetaPrincipalId,
            cliente.EsAdministrador);
    }

    public async Task<OperationResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!IsRegisterInputValid(request))
        {
            return OperationResult<RegisterResponse>.Invalid("Completá todos los campos del formulario");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedDocumento = request.Documento.Trim();

        var emailExists = await _dbContext.Clientes.AnyAsync(c => c.Email == normalizedEmail, cancellationToken)
                           || await _dbContext.Registros.AnyAsync(r => r.Email == normalizedEmail && r.Estado == "Pendiente",
                               cancellationToken);

        if (emailExists)
        {
            return OperationResult<RegisterResponse>.Conflict("Ya existe una solicitud o usuario con ese correo");
        }

        var registro = new ClienteRegistro
        {
            Id = Guid.NewGuid(),
            NombreCompleto = request.NombreCompleto.Trim(),
            Documento = normalizedDocumento,
            Email = normalizedEmail,
            Telefono = request.Telefono.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Clave),
            Estado = "Pendiente",
            CreadoEnUtc = DateTime.UtcNow
        };

        _dbContext.Registros.Add(registro);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new RegisterResponse(registro.Id, registro.Estado,
            "Recibimos tu solicitud. Te avisaremos cuando el alta esté lista.");

        return OperationResult<RegisterResponse>.Success(response);
    }

    public async Task<OperationResult> UpdatePasswordAsync(PasswordRecoveryRequest request, CancellationToken cancellationToken)
    {
        if (!IsRecoveryInputValid(request))
        {
            return OperationResult.Invalid("Necesitamos documento, correo y una nueva clave válida");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedDocumento = request.Documento.Trim();

        var cliente = await _dbContext.Clientes
            .FirstOrDefaultAsync(c => c.Email == normalizedEmail && c.Documento == normalizedDocumento, cancellationToken);

        if (cliente is null)
        {
            return OperationResult.NotFound("No encontramos un usuario que coincida con los datos ingresados");
        }

        cliente.PasswordHash = PasswordHasher.Hash(request.NuevaClave.Trim());
        await _dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    private static bool IsRegisterInputValid(RegisterRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.NombreCompleto)
               && !string.IsNullOrWhiteSpace(request.Documento)
               && !string.IsNullOrWhiteSpace(request.Email)
               && !string.IsNullOrWhiteSpace(request.Telefono)
               && !string.IsNullOrWhiteSpace(request.Clave)
               && request.Clave.Length >= 6;
    }

    private static bool IsRecoveryInputValid(PasswordRecoveryRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Documento)
               && !string.IsNullOrWhiteSpace(request.Email)
               && !string.IsNullOrWhiteSpace(request.NuevaClave)
               && request.NuevaClave.Length >= 6;
    }
}
