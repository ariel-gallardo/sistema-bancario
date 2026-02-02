namespace Tarjetas.Infrastructure;

public interface IUserContext
{
    Guid? ClienteId { get; }
    Guid? TarjetaPrincipalId { get; }
    bool EsAdministrador { get; }
}
