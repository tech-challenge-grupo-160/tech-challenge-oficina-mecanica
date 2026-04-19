using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;

public static class OrdemDeServicoMock
{
    public static OrdemDeServico Criar(
        int id = 3000,
        int clienteId = 1000,
        int veiculoId = 1000,
        StatusOrdemDeServico status = StatusOrdemDeServico.Recebida,
        string numero = "OS-20260419-3000")
    {
        return new OrdemDeServico
        {
            Id = id,
            Numero = numero,
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Status = status,
            DataAbertura = DateTime.UtcNow,
            ValorTotal = 0,
            DescricaoSolicitacao = "Solicitacao inicial de teste.",
            ObservacoesRecepcao = "Observacao inicial de teste."
        };
    }
}
