using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;

public static class PecaMock
{
    public static Peca Criar(
        int id = 1000,
        string nome = "Pastilha de Freio",
        string marca = "Cobreq",
        string modelo = "N-1234",
        decimal preco = 45m,
        int quantidadeEstoque = 10)
    {
        return Peca.Criar(nome, marca, modelo, preco, quantidadeEstoque)
            .WithId(id);
    }
}
