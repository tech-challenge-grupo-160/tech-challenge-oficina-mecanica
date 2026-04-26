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

    public static string ToSha256Hash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static string GenerateSecureHexToken(int byteLength = 32)
    {
        if (byteLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), "byteLength deve ser maior que zero.");
        }

        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToHexString(bytes);
    }

    public static bool FixedTimeEqualsHex(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        try
        {
            var leftBytes = Convert.FromHexString(left);
            var rightBytes = Convert.FromHexString(right);
            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (char.IsDigit(c))
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    public static string OnlyDigits(string? value, int minLength, int maxLength)
    {
        var digits = OnlyDigits(value);
        if (digits.Length < minLength || digits.Length > maxLength)
        {
            throw new ArgumentException($"Valor deve conter entre {minLength} e {maxLength} dígitos.");
        }

        return digits;
    }
}
