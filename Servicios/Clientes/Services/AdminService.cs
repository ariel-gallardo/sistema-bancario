using Clientes.Contracts;
using Clientes.Data;
using Clientes.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Services;

public class AdminService : IAdminService
{
    private readonly ClientesDbContext _dbContext;
    private readonly IUserContext _userContext;

    public AdminService(ClientesDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<OperationResult<AdminOverviewResponse>> GetOverviewAsync(CancellationToken cancellationToken)
    {
        if (!_userContext.EsAdministrador)
        {
            return OperationResult<AdminOverviewResponse>.Unauthorized("Necesitás permisos de administrador");
        }

        var totalClientes = await _dbContext.Clientes.CountAsync(cancellationToken);
        var pendientes = await _dbContext.Registros.CountAsync(r => r.Estado == "Pendiente", cancellationToken);
        var revisadas = await _dbContext.Registros.CountAsync(r => r.Estado != "Pendiente", cancellationToken);
        var ultimaSolicitud = await _dbContext.Registros
            .OrderByDescending(r => r.CreadoEnUtc)
            .Select(r => (DateTime?)r.CreadoEnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var response = new AdminOverviewResponse(totalClientes, pendientes, revisadas, ultimaSolicitud);
        return OperationResult<AdminOverviewResponse>.Success(response);
    }

    public async Task<OperationResult<IReadOnlyCollection<ClienteRegistroSummary>>> GetPendingRegistrosAsync(CancellationToken cancellationToken)
    {
        if (!_userContext.EsAdministrador)
        {
            return OperationResult<IReadOnlyCollection<ClienteRegistroSummary>>.Unauthorized("Necesitás permisos de administrador");
        }

        var registros = await _dbContext.Registros.AsNoTracking()
            .Where(r => r.Estado == "Pendiente")
            .OrderBy(r => r.CreadoEnUtc)
            .Select(r => new ClienteRegistroSummary(
                r.Id,
                r.NombreCompleto,
                r.Documento,
                r.Email,
                r.Telefono,
                r.Estado,
                r.CreadoEnUtc))
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyCollection<ClienteRegistroSummary>>.Success(registros);
    }

    public async Task<OperationResult> MarkRegistroAsReviewedAsync(Guid registroId, CancellationToken cancellationToken)
    {
        if (!_userContext.EsAdministrador)
        {
            return OperationResult.Unauthorized("Necesitás permisos de administrador");
        }

        var registro = await _dbContext.Registros.FirstOrDefaultAsync(r => r.Id == registroId, cancellationToken);
        if (registro is null)
        {
            return OperationResult.NotFound("No encontramos la solicitud");
        }

        registro.Estado = "Revisado";
        await _dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}
