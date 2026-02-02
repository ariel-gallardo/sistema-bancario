using Microsoft.EntityFrameworkCore;
using Tarjetas.Contracts;
using Tarjetas.Data;
using Tarjetas.Domain;
using Tarjetas.Infrastructure;

namespace Tarjetas.Services;

public class TarjetasService : ITarjetasService
{
    private readonly TarjetasDbContext _dbContext;
    private readonly IUserContext _userContext;

    public TarjetasService(TarjetasDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IResult> GetPrincipalMovimientosAsync(int? take, CancellationToken cancellationToken)
    {
        if (_userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var tarjeta = await FindClienteTarjetaAsync(
            _userContext.ClienteId.Value,
            _userContext.TarjetaPrincipalId,
            cancellationToken);

        if (tarjeta is null)
        {
            return Results.NotFound();
        }

        var response = await BuildTarjetaMovimientosResponseAsync(tarjeta, take, cancellationToken);

        return Results.Ok(response);
    }

    public async Task<IResult> GetTarjetasByCuentaAsync(Guid cuentaId, CancellationToken cancellationToken)
    {
        if (_userContext.ClienteId is null)
        {
            return Results.Unauthorized();
        }

        var tarjetas = await _dbContext.Tarjetas.AsNoTracking()
            .Where(t => t.ClienteId == _userContext.ClienteId && t.CuentaId == cuentaId)
            .OrderBy(t => t.Marca)
            .ToListAsync(cancellationToken);

        var response = tarjetas.Select(MapToResumen).ToList();

        return Results.Ok(response);
    }

    public async Task<IResult> GetMovimientosByTarjetaAsync(Guid tarjetaId, int? take, CancellationToken cancellationToken)
    {
        var tarjeta = await _dbContext.Tarjetas.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tarjetaId, cancellationToken);

        if (tarjeta is null)
        {
            return Results.NotFound();
        }

        if (!_userContext.EsAdministrador && tarjeta.ClienteId != _userContext.ClienteId)
        {
            return Results.NotFound();
        }

        var response = await BuildTarjetaMovimientosResponseAsync(tarjeta, take, cancellationToken);
        return Results.Ok(response);
    }

    public async Task<IResult> AddMovimientoAsync(Guid tarjetaId, CrearMovimientoRequest request, CancellationToken cancellationToken)
    {
        if (!_userContext.EsAdministrador)
        {
            return Results.Forbid();
        }

        var tarjeta = await _dbContext.Tarjetas
            .FirstOrDefaultAsync(t => t.Id == tarjetaId, cancellationToken);

        if (tarjeta is null)
        {
            return Results.NotFound();
        }

        var movimiento = new MovimientoTarjeta
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjeta.Id,
            ClienteId = tarjeta.ClienteId,
            Comercio = request.Comercio,
            Descripcion = request.Descripcion,
            Categoria = request.Categoria,
            Importe = request.Importe,
            Fecha = request.Fecha ?? DateTime.UtcNow
        };

        tarjeta.SaldoUtilizado += movimiento.Importe;

        _dbContext.Movimientos.Add(movimiento);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new MovimientoTarjetaResponse(
            movimiento.Id,
            movimiento.Comercio,
            movimiento.Descripcion,
            movimiento.Categoria,
            movimiento.Importe,
            movimiento.Fecha);

        return Results.Created($"/api/tarjetas/{tarjetaId}/movimientos/{movimiento.Id}", response);
    }

    private async Task<TarjetaCredito?> FindClienteTarjetaAsync(Guid clienteId, Guid? preferredTarjetaId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tarjetas.AsNoTracking()
            .Where(t => t.ClienteId == clienteId);

        if (preferredTarjetaId is Guid preferida)
        {
            var match = await query.FirstOrDefaultAsync(t => t.Id == preferida, cancellationToken);
            if (match is not null)
            {
                return match;
            }
        }

        return await query
            .OrderByDescending(t => t.Limite)
            .ThenBy(t => t.Marca)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<TarjetaMovimientosResponse> BuildTarjetaMovimientosResponseAsync(
        TarjetaCredito tarjeta,
        int? take,
        CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(take.GetValueOrDefault(5), 1, 20);
        var movimientos = await _dbContext.Movimientos.AsNoTracking()
            .Where(m => m.TarjetaId == tarjeta.Id)
            .OrderByDescending(m => m.Fecha)
            .Take(limit)
            .Select(m => new MovimientoTarjetaResponse(
                m.Id,
                m.Comercio,
                m.Descripcion,
                m.Categoria,
                m.Importe,
                m.Fecha))
            .ToListAsync(cancellationToken);

        var disponible = Math.Max(0, tarjeta.Limite - tarjeta.SaldoUtilizado);

        return new TarjetaMovimientosResponse(
            tarjeta.Id,
            tarjeta.CuentaId,
            tarjeta.Marca,
            tarjeta.NumeroEnmascarado,
            tarjeta.Limite,
            tarjeta.SaldoUtilizado,
            disponible,
            tarjeta.PagoMinimo,
            tarjeta.Cierre,
            tarjeta.Vencimiento,
            movimientos);
    }

    private TarjetaResumenResponse MapToResumen(TarjetaCredito tarjeta)
    {
        var disponible = Math.Max(0, tarjeta.Limite - tarjeta.SaldoUtilizado);
        var esPrincipal = _userContext.TarjetaPrincipalId is not null && tarjeta.Id == _userContext.TarjetaPrincipalId;

        return new TarjetaResumenResponse(
            tarjeta.Id,
            tarjeta.CuentaId,
            tarjeta.Marca,
            tarjeta.NumeroEnmascarado,
            tarjeta.Limite,
            tarjeta.SaldoUtilizado,
            disponible,
            esPrincipal);
    }
}
