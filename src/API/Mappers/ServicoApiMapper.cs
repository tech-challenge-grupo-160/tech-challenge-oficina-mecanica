using Fiap.TechChallenge.OficinaMecanica.API.Requests.Servicos;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class ServicoApiMapper
{
    public static CriarServicoCommand ToCommand(this CriarServicoRequest request)
    {
        return new CriarServicoCommand
        {
            Nome = request.Nome,
            Descricao = request.Descricao,
            Preco = request.Preco,
            TempoEstimado = request.TempoEstimado
        };
    }

    public static AtualizarServicoCommand ToCommand(this AtualizarServicoRequest request, int id)
    {
        return new AtualizarServicoCommand
        {
            Id = id,
            Nome = request.Nome,
            Descricao = request.Descricao,
            Preco = request.Preco,
            TempoEstimado = request.TempoEstimado
        };
    }

    public static ObterServicoPorIdQuery ToServicoQueryById(this int id)
    {
        return new ObterServicoPorIdQuery
        {
            Id = id
        };
    }

    public static DeletarServicoCommand ToDeleteServicoCommand(this int id)
    {
        return new DeletarServicoCommand
        {
            Id = id
        };
    }

    public static ServicoResponse ToResponse(this ServicoResult result)
    {
        return new ServicoResponse
        {
            Id = result.Id,
            Nome = result.Nome,
            Descricao = result.Descricao,
            Preco = result.Preco,
            TempoEstimado = result.TempoEstimado
        };
    }

    public static IEnumerable<ServicoResponse> ToResponse(this IEnumerable<ServicoResult> results)
    {
        return results.Select(servico => servico.ToResponse()).ToArray();
    }
}
