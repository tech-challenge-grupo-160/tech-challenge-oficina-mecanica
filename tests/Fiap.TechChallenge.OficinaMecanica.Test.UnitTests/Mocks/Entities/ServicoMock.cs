using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;

public static class ServicoMock
{
    public static Servico Criar(
        int id = 1000,
        string nome = "Alinhamento",
        string descricao = "Servico de alinhamento",
        decimal preco = 150m,
        int tempoEstimado = 30)
    {
        return Servico.Criar(nome, descricao, preco, tempoEstimado)
            .WithId(id);
    }
}
