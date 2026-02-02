using Microsoft.EntityFrameworkCore;
using Pagos.Contracts;
using Pagos.Data;
using Pagos.Infrastructure;

namespace Pagos.Services;

public class PagosService : IPagosService
{
    private readonly PagosDbContext _dbContext;
    private readonly IUserContext _userContext;

    public PagosService(PagosDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IResult> GetPagosProgramadosAsync(CancellationToken cancellationToken)
    {
        if (_userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var programados = await _dbContext.PagosProgramados.AsNoTracking()
            .Where(p => p.ClienteId == _userContext.ClienteId)
            .OrderBy(p => p.FechaProgramada)
            .Select(p => new PagoProgramadoResponse(
                p.Id,
                p.Empresa,
                p.Descripcion,
                p.Importe,
                p.Moneda,
                p.FechaProgramada,
                p.EsDebitoAutomatico,
                p.Estado))
            .ToListAsync(cancellationToken);

        return Results.Ok(new PagosProgramadosResponse(programados));
    }

    public async Task<IResult> GetPagosHistorialAsync(int? take, CancellationToken cancellationToken)
    {
        if (_userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var limit = Math.Clamp(take.GetValueOrDefault(5), 1, 30);

        var historial = await _dbContext.PagosHistoricos.AsNoTracking()
            .Where(p => p.ClienteId == _userContext.ClienteId)
            .OrderByDescending(p => p.FechaPago)
            .Take(limit)
            .Select(p => new PagoHistoricoResponse(
                p.Id,
                p.Empresa,
                p.Categoria,
                p.Importe,
                p.Moneda,
                p.FechaPago,
                p.MedioPago,
                p.FueDebitoAutomatico))
            .ToListAsync(cancellationToken);

        return Results.Ok(new PagosHistorialResponse(historial));
    }
}
