namespace Pagos.Domain;

public class PagoHistorico
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public required string Empresa { get; set; }
    public required string Categoria { get; set; }
    public decimal Importe { get; set; }
    public required string Moneda { get; set; }
    public DateTime FechaPago { get; set; }
    public required string MedioPago { get; set; }
    public bool FueDebitoAutomatico { get; set; }
}
