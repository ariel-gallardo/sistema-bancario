using System.Security.Cryptography;
using System.Text;

namespace Clientes.Security;

public static class PasswordHasher
{
    public static string Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string input, string hash) =>
        Hash(input).Equals(hash, StringComparison.OrdinalIgnoreCase);
}
