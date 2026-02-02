namespace Tarjetas.Contracts;

public record CrearMovimientoRequest(
    string Comercio,
    string Descripcion,
    string Categoria,
    decimal Importe,
    DateTime? Fecha);
