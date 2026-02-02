namespace Pagos.Services;

public interface IPagosService
{
    Task<IResult> GetPagosProgramadosAsync(CancellationToken cancellationToken);
    Task<IResult> GetPagosHistorialAsync(int? take, CancellationToken cancellationToken);
}
