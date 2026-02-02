namespace Pagos.Domain;

public class PagoProgramado
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public required string Empresa { get; set; }
    public required string Descripcion { get; set; }
    public decimal Importe { get; set; }
    public required string Moneda { get; set; }
    public DateTime FechaProgramada { get; set; }
    public bool EsDebitoAutomatico { get; set; }
    public required string Estado { get; set; }
}
