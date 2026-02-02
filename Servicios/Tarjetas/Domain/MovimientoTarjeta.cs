namespace Tarjetas.Domain;

public class MovimientoTarjeta
{
    public Guid Id { get; set; }
    public Guid TarjetaId { get; set; }
    public Guid ClienteId { get; set; }
    public required string Comercio { get; set; }
    public required string Descripcion { get; set; }
    public decimal Importe { get; set; }
    public DateTime Fecha { get; set; }
    public required string Categoria { get; set; }
    public TarjetaCredito? Tarjeta { get; set; }
}
