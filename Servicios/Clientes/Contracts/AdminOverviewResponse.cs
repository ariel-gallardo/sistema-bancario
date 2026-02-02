namespace Clientes.Contracts;

public record AdminOverviewResponse(
    int TotalClientes,
    int SolicitudesPendientes,
    int SolicitudesRevisadas,
    DateTime? UltimaSolicitudUtc);
