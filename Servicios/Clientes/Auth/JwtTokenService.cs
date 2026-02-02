using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clientes.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clientes.Auth;

public class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public string CreateToken(Cliente cliente)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, cliente.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, cliente.Email),
            new("clienteId", cliente.Id.ToString()),
            new("cuentaPrincipalId", cliente.CuentaPrincipalId.ToString()),
            new("tarjetaPrincipalId", cliente.TarjetaPrincipalId.ToString()),
            new("nombre", cliente.NombreCompleto)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
