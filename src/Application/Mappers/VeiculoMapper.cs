using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Mappers;

public static class VeiculoMapper
{
    public static VeiculoResult ToResult(this Veiculo veiculo)
    {
        return new VeiculoResult
        {
            Id = veiculo.Id,
            Placa = veiculo.Placa.Valor,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            ClienteId = veiculo.ClienteId
        };
    }
}
