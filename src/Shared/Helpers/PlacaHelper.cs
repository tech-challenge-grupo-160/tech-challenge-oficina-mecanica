namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class PlacaHelper
{
    public static string Normalizar(string? placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            throw new ArgumentException("Placa obrigatoria.");
        }

        var normalizada = RemoverSeparadores(placa).ToUpperInvariant();
        if (!IsValid(normalizada))
        {
            throw new ArgumentException("Placa invalida.");
        }

        return normalizada;
    }

    public static bool IsValid(string? placa)
    {
        var normalizada = RemoverSeparadores(placa).ToUpperInvariant();
        if (normalizada.Length != 7)
        {
            return false;
        }

        return IsFormatoAntigo(normalizada) || IsFormatoMercosul(normalizada);
    }

    private static bool IsFormatoAntigo(string placa)
    {
        return IsLetter(placa[0]) &&
               IsLetter(placa[1]) &&
               IsLetter(placa[2]) &&
               char.IsDigit(placa[3]) &&
               char.IsDigit(placa[4]) &&
               char.IsDigit(placa[5]) &&
               char.IsDigit(placa[6]);
    }

    private static bool IsFormatoMercosul(string placa)
    {
        return IsLetter(placa[0]) &&
               IsLetter(placa[1]) &&
               IsLetter(placa[2]) &&
               char.IsDigit(placa[3]) &&
               IsLetter(placa[4]) &&
               char.IsDigit(placa[5]) &&
               char.IsDigit(placa[6]);
    }

    private static string RemoverSeparadores(string? placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            return string.Empty;
        }

        var buffer = new char[7];
        var index = 0;

        foreach (var caractere in placa.Trim())
        {
            if (caractere is '-' or ' ')
            {
                continue;
            }

            if (index == buffer.Length)
            {
                return string.Empty;
            }

            buffer[index++] = caractere;
        }

        return new string(buffer, 0, index);
    }

    private static bool IsLetter(char value) => value is >= 'A' and <= 'Z';
}
