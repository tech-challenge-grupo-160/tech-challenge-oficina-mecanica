using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Mappers;

public static class ServicoMapper
{
    public static ServicoResult ToResult(this Servico servico)
    {
        return new ServicoResult
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Preco = servico.Preco,
            TempoEstimado = servico.TempoEstimado
        };
    }
}
