using Clientes.Domain;

namespace Clientes.Auth;

public interface IJwtTokenService
{
    string CreateToken(Cliente cliente);
}
