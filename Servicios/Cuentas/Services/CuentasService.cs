using Cuentas.Contracts;
using Cuentas.Data;
using Microsoft.EntityFrameworkCore;

namespace Cuentas.Services;

public class CuentasService(CuentasDbContext dbContext) : ICuentasService
{
    private readonly CuentasDbContext _dbContext = dbContext;

    public async Task<AccountSummaryResponse?> GetCuentaPrincipalAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var principal = await _dbContext.Cuentas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.EsPrincipal, cancellationToken);

        return principal is null
            ? null
            : await BuildAccountSummaryAsync(clienteId, principal.Id, cancellationToken);
    }

    public Task<AccountSummaryResponse?> GetCuentaByIdAsync(Guid clienteId, Guid cuentaId, CancellationToken cancellationToken) =>
        BuildAccountSummaryAsync(clienteId, cuentaId, cancellationToken);

    public async Task<bool> SetCuentaFavoritaAsync(Guid clienteId, Guid cuentaId, CancellationToken cancellationToken)
    {
        var cuentasCliente = await _dbContext.Cuentas
            .Where(c => c.ClienteId == clienteId)
            .ToListAsync(cancellationToken);

        if (!cuentasCliente.Any(c => c.Id == cuentaId))
        {
            return false;
        }

        foreach (var cuenta in cuentasCliente)
        {
            cuenta.EsFavorita = cuenta.Id == cuentaId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ClearCuentaFavoritaAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var favoritas = await _dbContext.Cuentas
            .Where(c => c.ClienteId == clienteId && c.EsFavorita)
            .ToListAsync(cancellationToken);

        if (favoritas.Count == 0)
        {
            return false;
        }

        foreach (var cuenta in favoritas)
        {
            cuenta.EsFavorita = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AccountSummaryResponse?> BuildAccountSummaryAsync(Guid clienteId, Guid cuentaId, CancellationToken cancellationToken)
    {
        var cuenta = await _dbContext.Cuentas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Id == cuentaId, cancellationToken);

        if (cuenta is null)
        {
            return null;
        }

        var otrasCuentas = await _dbContext.Cuentas.AsNoTracking()
            .Where(c => c.ClienteId == clienteId && c.Id != cuenta.Id)
            .OrderByDescending(c => c.EsPrincipal)
            .Select(c => new AccountSnapshot(
                c.Id,
                c.Alias,
                c.Moneda,
                c.SaldoActual,
                c.EsPrincipal,
                c.EsFavorita))
            .ToListAsync(cancellationToken);

        var saldoDisponible = cuenta.SaldoActual + cuenta.LimiteDescubierto;

        return new AccountSummaryResponse(
            cuenta.Id,
            cuenta.Alias,
            cuenta.Banco,
            cuenta.Numero,
            cuenta.Moneda,
            cuenta.SaldoActual,
            saldoDisponible,
            cuenta.UltimaActualizacion,
            cuenta.EsPrincipal,
            cuenta.EsFavorita,
            otrasCuentas);
    }
}
