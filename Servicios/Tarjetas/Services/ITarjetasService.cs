using Tarjetas.Contracts;

namespace Tarjetas.Services;

public interface ITarjetasService
{
    Task<IResult> GetPrincipalMovimientosAsync(int? take, CancellationToken cancellationToken);
    Task<IResult> AddMovimientoAsync(Guid tarjetaId, CrearMovimientoRequest request, CancellationToken cancellationToken);
}
