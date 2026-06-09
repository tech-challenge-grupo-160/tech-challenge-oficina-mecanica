using Fiap.TechChallenge.OficinaMecanica.API.Requests.Pecas;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class PecaApiMapper
{
    public static CriarPecaCommand ToCommand(this CriarPecaRequest request)
    {
        return new CriarPecaCommand
        {
            Nome = request.Nome,
            Marca = request.Marca,
            Modelo = request.Modelo,
            Preco = request.Preco,
            QuantidadeEstoque = request.QuantidadeEstoque
        };
    }

    public static AtualizarPecaCommand ToCommand(this AtualizarPecaRequest request, int id)
    {
        return new AtualizarPecaCommand
        {
            Id = id,
            Nome = request.Nome,
            Marca = request.Marca,
            Modelo = request.Modelo,
            Preco = request.Preco,
            QuantidadeEstoque = request.QuantidadeEstoque
        };
    }

    public static ObterPecaPorIdQuery ToPecaQueryById(this int id)
    {
        return new ObterPecaPorIdQuery
        {
            Id = id
        };
    }

    public static DeletarPecaCommand ToDeletePecaCommand(this int id)
    {
        return new DeletarPecaCommand
        {
            Id = id
        };
    }

    public static PecaResponse ToResponse(this PecaResult result)
    {
        return new PecaResponse
        {
            Id = result.Id,
            Nome = result.Nome,
            Marca = result.Marca,
            Modelo = result.Modelo,
            Preco = result.Preco,
            QuantidadeEstoque = result.QuantidadeEstoque
        };
    }

    public static IEnumerable<PecaResponse> ToResponse(this IEnumerable<PecaResult> results)
    {
        return results.Select(peca => peca.ToResponse()).ToArray();
    }
}
