using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class PlacaHelper
{
    public static string Normalizar(string? placa)
    {
        return PlacaVeiculo.Parse(placa ?? string.Empty).Valor;
    }

    public static bool IsValid(string? placa)
    {
        return PlacaVeiculo.IsValid(placa);
    }
}
