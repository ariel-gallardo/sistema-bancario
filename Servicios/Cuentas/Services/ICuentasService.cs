using Cuentas.Contracts;

namespace Cuentas.Services;

public interface ICuentasService
{
    Task<AccountSummaryResponse?> GetCuentaPrincipalAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<AccountSummaryResponse?> GetCuentaByIdAsync(Guid clienteId, Guid cuentaId, CancellationToken cancellationToken);
    Task<bool> SetCuentaFavoritaAsync(Guid clienteId, Guid cuentaId, CancellationToken cancellationToken);
    Task<bool> ClearCuentaFavoritaAsync(Guid clienteId, CancellationToken cancellationToken);
}
