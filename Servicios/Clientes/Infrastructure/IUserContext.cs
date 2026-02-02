namespace Clientes.Infrastructure;

public interface IUserContext
{
    Guid? ClienteId { get; }
    bool EsAdministrador { get; }
}
