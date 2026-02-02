namespace Tarjetas.Contracts;

public record TarjetaMovimientosResponse(
    Guid TarjetaId,
    string Marca,
    string NumeroEnmascarado,
    decimal Limite,
    decimal SaldoUtilizado,
    decimal Disponible,
    decimal PagoMinimo,
    DateTime FechaCierre,
    DateTime FechaVencimiento,
    IReadOnlyCollection<MovimientoTarjetaResponse> Movimientos);

public record MovimientoTarjetaResponse(
    Guid MovimientoId,
    string Comercio,
    string Descripcion,
    string Categoria,
    decimal Importe,
    DateTime Fecha);
