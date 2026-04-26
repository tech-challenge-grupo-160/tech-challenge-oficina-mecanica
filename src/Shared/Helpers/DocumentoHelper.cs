using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class DocumentoHelper
{
    public static string NormalizarCpf(string cpf)
    {
        var documento = Documento.Parse(cpf);
        if (!documento.IsCpf)
        {
            throw new ArgumentException("CPF invalido.");
        }

        return documento.Valor;
    }

    public static string NormalizarDocumento(string documento)
    {
        return Documento.Parse(documento).Valor;
    }

    public static string NormalizarCnpj(string cnpj)
    {
        var documento = Documento.Parse(cnpj);
        if (!documento.IsCnpj)
        {
            throw new ArgumentException("CNPJ invalido.");
        }

        return documento.Valor;
    }

    public static bool ValidarCpf(string? cpf)
    {
        try
        {
            var documento = Documento.Parse(cpf ?? string.Empty);
            return documento.IsCpf;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static bool ValidarCnpj(string? cnpj)
    {
        try
        {
            var documento = Documento.Parse(cnpj ?? string.Empty);
            return documento.IsCnpj;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
