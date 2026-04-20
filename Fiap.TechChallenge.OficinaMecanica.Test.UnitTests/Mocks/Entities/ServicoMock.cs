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
        return new Servico
        {
            Id = id,
            Nome = nome,
            Descricao = descricao,
            Preco = preco,
            TempoEstimado = tempoEstimado
        };
    }
}
