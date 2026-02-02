namespace Tarjetas.Infrastructure;

public interface IUserContext
{
    Guid? ClienteId { get; }
    bool EsAdministrador { get; }
}
