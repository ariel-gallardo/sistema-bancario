using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tarjetas.Tests.Infrastructure;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>();

        if (Guid.TryParse(Request.Headers["x-test-cliente-id"], out var clienteId))
        {
            claims.Add(new Claim("clienteId", clienteId.ToString()));
        }

        if (Guid.TryParse(Request.Headers["x-test-tarjeta-id"], out var tarjetaId))
        {
            claims.Add(new Claim("tarjetaPrincipalId", tarjetaId.ToString()));
        }

        var isAdmin = Request.Headers["x-test-is-admin"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(isAdmin))
        {
            claims.Add(new Claim("esAdmin", isAdmin));
        }

        if (claims.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test identity headers."));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
