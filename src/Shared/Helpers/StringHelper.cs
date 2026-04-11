using System.Security.Cryptography;
using System.Text;

namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class StringHelper
{
    public static string ToMd5Hash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = md5.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
