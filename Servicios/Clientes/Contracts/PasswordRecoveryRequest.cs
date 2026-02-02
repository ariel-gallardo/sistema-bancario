namespace Clientes.Contracts;

public record PasswordRecoveryRequest(
    string Documento,
    string Email,
    string NuevaClave);
