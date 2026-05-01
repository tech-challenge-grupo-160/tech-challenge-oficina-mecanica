using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class TelefoneHelper
{
    public static string Normalizar(string telefone)
    {
        return Telefone.Parse(telefone).Valor;
    }

    public static bool IsValid(string telefone)
    {
        return Telefone.IsValid(telefone);
    }
}
