using Clientes.Contracts;

namespace Clientes.Services;

public interface IAdminService
{
    Task<OperationResult<AdminOverviewResponse>> GetOverviewAsync(CancellationToken cancellationToken);
    Task<OperationResult<IReadOnlyCollection<ClienteRegistroSummary>>> GetPendingRegistrosAsync(CancellationToken cancellationToken);
    Task<OperationResult> MarkRegistroAsReviewedAsync(Guid registroId, CancellationToken cancellationToken);
}
