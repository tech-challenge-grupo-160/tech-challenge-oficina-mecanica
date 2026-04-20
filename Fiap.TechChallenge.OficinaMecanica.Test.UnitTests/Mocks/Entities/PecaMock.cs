using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;

public static class PecaMock
{
    public static Peca Criar(
        int id = 1000,
        string nome = "Pastilha de Freio",
        decimal preco = 45m,
        int quantidadeEstoque = 10)
    {
        return new Peca
        {
            Id = id,
            Nome = nome,
            Preco = preco,
            QuantidadeEstoque = quantidadeEstoque
        };
    }
}
