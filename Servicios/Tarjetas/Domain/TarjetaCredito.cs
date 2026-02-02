namespace Tarjetas.Domain;

public class TarjetaCredito
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid CuentaId { get; set; }
    public required string NumeroEnmascarado { get; set; }
    public required string Marca { get; set; }
    public decimal Limite { get; set; }
    public decimal SaldoUtilizado { get; set; }
    public decimal PagoMinimo { get; set; }
    public DateTime Cierre { get; set; }
    public DateTime Vencimiento { get; set; }
    public ICollection<MovimientoTarjeta> Movimientos { get; set; } = new List<MovimientoTarjeta>();
}
