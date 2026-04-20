using System.Text;

namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class DocumentoHelper
{
    public static string NormalizarCpf(string cpf)
    {
        var digits = ExtrairDigitos(cpf);
        if (!ValidarCpfDigits(digits))
        {
            throw new ArgumentException("CPF invalido.");
        }

        return digits;
    }

    public static string NormalizarDocumento(string documento)
    {
        var cleaned = StringHelper.OnlyDigits(documento);
        if (cleaned.Length == 11)
        {
            return NormalizarCpf(documento);
        }

        if (cleaned.Length == 14)
        {
            return NormalizarCnpj(documento);
        }

        throw new ArgumentException("Documento deve conter 11 (CPF) ou 14 (CNPJ) dígitos.");
    }

    public static string NormalizarCnpj(string cnpj)
    {
        var normalized = LimparCnpj(cnpj);
        if (!ValidarCnpjDigits(normalized))
        {
            throw new ArgumentException("CNPJ invalido.");
        }

        return normalized;
    }

    public static bool ValidarCpf(string? cpf)
    {
        var digits = ExtrairDigitos(cpf);
        return ValidarCpfDigits(digits);
    }

    public static bool ValidarCnpj(string? cnpj)
    {
        var normalized = LimparCnpj(cnpj);
        return ValidarCnpjDigits(normalized);
    }

    private static string ExtrairDigitos(string? valor) => StringHelper.OnlyDigits(valor);
    private static string LimparCnpj(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(valor.Length);
        foreach (var c in valor)
        {
            if (char.IsDigit(c))
            {
                builder.Append(c);
            }
            else if (char.IsLetter(c))
            {
                builder.Append(char.ToUpperInvariant(c));
            }
        }

        return builder.ToString();
    }

    private static bool ValidarCpfDigits(string digits)
    {
        if (digits.Length != 11 || digits.Distinct().Count() == 1)
        {
            return false;
        }

        var dv1 = CalcularDigito(digits[..9], new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 });
        var dv2 = CalcularDigito(digits[..10], new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 });

        return digits[9] == dv1 && digits[10] == dv2;
    }

    private static bool ValidarCnpjDigits(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
        {
            return false;
        }

        if (!char.IsDigit(cnpj[12]) || !char.IsDigit(cnpj[13]))
        {
            return false;
        }

        var dv1 = CalcularDigitoCnpj(cnpj[..12], new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
        var dv2 = CalcularDigitoCnpj(cnpj[..13], new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });

        return cnpj[12] == dv1 && cnpj[13] == dv2;
    }

    private static char CalcularDigito(string baseDigits, int[] pesos)
    {
        var soma = baseDigits.Select((t, i) => (t - '0') * pesos[i]).Sum();
        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;
        return (char)('0' + digito);
    }

    private static char CalcularDigitoCnpj(string baseChars, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < baseChars.Length; i++)
        {
            var valor = ConverterCnpjCharParaNumero(baseChars[i]);
            soma += valor * pesos[i];
        }

        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;
        return (char)('0' + digito);
    }

    private static int ConverterCnpjCharParaNumero(char caractere)
    {
        if (char.IsDigit(caractere))
        {
            return caractere - '0';
        }

        if (char.IsLetter(caractere))
        {
            var upper = char.ToUpperInvariant(caractere);
            return upper - '0';
        }

        throw new ArgumentException("Caracter invalido para CNPJ.");
    }
}
