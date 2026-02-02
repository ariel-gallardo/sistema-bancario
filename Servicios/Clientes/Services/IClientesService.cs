using Clientes.Contracts;

namespace Clientes.Services;

public interface IClientesService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<ClienteProfileResponse?> GetProfileAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<OperationResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<OperationResult> UpdatePasswordAsync(PasswordRecoveryRequest request, CancellationToken cancellationToken);
}
