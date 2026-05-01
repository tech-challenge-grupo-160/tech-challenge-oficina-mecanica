using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

public readonly record struct Telefone
{
    public string Valor { get; }

    private Telefone(string valor)
    {
        Valor = valor;
    }

    public static Telefone Parse(string valor)
    {
        var digitos = StringHelper.OnlyDigits(valor);
        if (!IsValid(digitos))
        {
            throw new ArgumentException("Telefone invalido.");
        }

        return new Telefone(digitos);
    }

    public static bool IsValid(string? valor)
    {
        var digitos = StringHelper.OnlyDigits(valor);
        return digitos.Length is >= 10 and <= 11;
    }

    public override string ToString() => Valor;
}
