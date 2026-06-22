using System.Text.RegularExpressions;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

public readonly record struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }

    public static Email Parse(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("E-mail não pode ser vazio.");
        }

        var valorFormatado = valor.Trim().ToLowerInvariant();

        if (!IsValid(valorFormatado))
        {
            throw new ArgumentException("E-mail inválido.");
        }

        return new Email(valorFormatado);
    }

    internal static Email FromDatabase(string valor)
    {
        return new Email(valor);
    }

    public static bool IsValid(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        return EmailRegex.IsMatch(valor.Trim());
    }

    public override string ToString() => Valor;
}