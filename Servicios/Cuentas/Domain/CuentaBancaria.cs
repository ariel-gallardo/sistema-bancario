namespace Cuentas.Domain;

public class CuentaBancaria
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public required string Alias { get; set; }
    public required string Banco { get; set; }
    public required string Numero { get; set; }
    public required string Moneda { get; set; }
    public decimal SaldoActual { get; set; }
    public decimal LimiteDescubierto { get; set; }
    public bool EsPrincipal { get; set; }
    public bool EsFavorita { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}
