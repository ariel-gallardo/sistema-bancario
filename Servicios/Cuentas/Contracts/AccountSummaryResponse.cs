namespace Cuentas.Contracts;

public record AccountSummaryResponse(
    Guid CuentaId,
    string Alias,
    string Banco,
    string Numero,
    string Moneda,
    decimal SaldoActual,
    decimal SaldoDisponible,
    DateTime UltimaActualizacion,
    IReadOnlyCollection<AccountSnapshot> OtrasCuentas);

public record AccountSnapshot(
    Guid CuentaId,
    string Alias,
    string Moneda,
    decimal SaldoActual,
    bool EsPrincipal);
