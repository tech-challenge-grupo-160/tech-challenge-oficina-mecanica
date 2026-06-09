using Fiap.TechChallenge.OficinaMecanica.API.Requests.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class VeiculoApiMapper
{
    public static CriarVeiculoCommand ToCommand(this CriarVeiculoRequest request)
    {
        return new CriarVeiculoCommand
        {
            Placa = request.Placa,
            Marca = request.Marca,
            Modelo = request.Modelo,
            Ano = request.Ano,
            CpfCnpj = request.CpfCnpj
        };
    }

    public static CriarVeiculoParaClienteCommand ToCommand(this CriarVeiculoParaClienteRequest request, string cpfCnpj)
    {
        return new CriarVeiculoParaClienteCommand
        {
            CpfCnpj = cpfCnpj,
            Placa = request.Placa,
            Marca = request.Marca,
            Modelo = request.Modelo,
            Ano = request.Ano
        };
    }

    public static AtualizarVeiculoCommand ToCommand(this AtualizarVeiculoRequest request, int id)
    {
        return new AtualizarVeiculoCommand
        {
            Id = id,
            Marca = request.Marca,
            Modelo = request.Modelo,
            Ano = request.Ano
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

    public static VeiculoResponse ToResponse(this VeiculoResult result)
    {
        return new VeiculoResponse
        {
            Id = result.Id,
            Placa = result.Placa,
            Marca = result.Marca,
            Modelo = result.Modelo,
            Ano = result.Ano,
            ClienteId = result.ClienteId
        };
    }

    public static IEnumerable<VeiculoResponse> ToResponse(this IEnumerable<VeiculoResult> results)
    {
        return results.Select(veiculo => veiculo.ToResponse()).ToArray();
    }
}
