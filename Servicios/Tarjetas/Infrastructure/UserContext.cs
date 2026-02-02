using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Tarjetas.Infrastructure;

public class UserContext : IUserContext
{
    public Guid? ClienteId { get; }
    public bool EsAdministrador { get; }

    public UserContext(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        if (user is null)
        {
            return;
        }

        var clienteIdValue = user.FindFirstValue("clienteId");
        if (Guid.TryParse(clienteIdValue, out var clienteId))
        {
            ClienteId = clienteId;
        }

        EsAdministrador = string.Equals(user.FindFirstValue("esAdmin"), "true", StringComparison.OrdinalIgnoreCase);
    }
}
