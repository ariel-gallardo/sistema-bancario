namespace Pagos.Contracts;

public record PagoProgramadoResponse(
    Guid PagoId,
    string Empresa,
    string Descripcion,
    decimal Importe,
    string Moneda,
    DateTime FechaProgramada,
    bool EsDebitoAutomatico,
    string Estado);

public record PagosProgramadosResponse(IReadOnlyCollection<PagoProgramadoResponse> Programados);

public record PagoHistoricoResponse(
    Guid PagoId,
    string Empresa,
    string Categoria,
    decimal Importe,
    string Moneda,
    DateTime FechaPago,
    string MedioPago,
    bool FueDebitoAutomatico);

public record PagosHistorialResponse(IReadOnlyCollection<PagoHistoricoResponse> Historial);
