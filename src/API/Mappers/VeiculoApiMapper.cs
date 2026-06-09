using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class VeiculoApiMapper
{
    public static CriarVeiculoCommand ToCommand(this CriarVeiculoDto dto)
    {
        return new CriarVeiculoCommand
        {
            Placa = dto.Placa,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Ano = dto.Ano,
            CpfCnpj = dto.CpfCnpj
        };
    }

    public static CriarVeiculoParaClienteCommand ToCommand(this CriarVeiculoParaClienteDto dto, string cpfCnpj)
    {
        return new CriarVeiculoParaClienteCommand
        {
            CpfCnpj = cpfCnpj,
            Placa = dto.Placa,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Ano = dto.Ano
        };
    }

    public static AtualizarVeiculoCommand ToCommand(this AtualizarVeiculoDto dto, int id)
    {
        return new AtualizarVeiculoCommand
        {
            Id = id,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Ano = dto.Ano
        };
    }

    public static ObterVeiculoPorIdQuery ToQueryById(this int id)
    {
        return new ObterVeiculoPorIdQuery
        {
            Id = id
        };
    }

    public static ObterVeiculoPorPlacaQuery ToQueryByPlaca(this string placa)
    {
        return new ObterVeiculoPorPlacaQuery
        {
            Placa = placa
        };
    }

    public static DeletarVeiculoCommand ToDeleteCommand(this int id)
    {
        return new DeletarVeiculoCommand
        {
            Id = id
        };
    }
}
