using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Mappers;

public static class PecaMapper
{
    public static PecaResult ToResult(this Peca peca)
    {
        return new PecaResult
        {
            Id = peca.Id,
            Nome = peca.Nome,
            Marca = peca.Marca,
            Modelo = peca.Modelo,
            Preco = peca.Preco,
            QuantidadeEstoque = peca.QuantidadeEstoque
        };
    }
}
