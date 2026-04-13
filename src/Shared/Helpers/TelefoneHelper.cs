namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class TelefoneHelper
{
    public static string Normalizar(string telefone)
    {
        var digitos = StringHelper.OnlyDigits(telefone);
        if (digitos.Length < 10 || digitos.Length > 11)
        {
            throw new ArgumentException("Telefone inválido.");
        }

        return digitos;
    }
}
