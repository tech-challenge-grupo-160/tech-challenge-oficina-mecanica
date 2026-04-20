namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class TelefoneHelper
{
    public static string Normalizar(string telefone)
    {
        var digitos = StringHelper.OnlyDigits(telefone);
        if (!IsValid(digitos))
        {
            throw new ArgumentException("Telefone inválido.");
        }

        return digitos;
    }

    public static bool IsValid(string telefone)
    {
        var digitos = StringHelper.OnlyDigits(telefone);
        return digitos.Length is >= 10 and <= 11;
    }
}
