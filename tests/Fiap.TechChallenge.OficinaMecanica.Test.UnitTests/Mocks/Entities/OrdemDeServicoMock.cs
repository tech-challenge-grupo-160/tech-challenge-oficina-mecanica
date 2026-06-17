using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

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
        return OrdemDeServico.Restaurar(
                numero,
                $"AC-TEST-{id}",
                $"HASH-TEST-{id}",
                clienteId,
                veiculoId,
                "Solicitacao inicial de teste.",
                "Observacao inicial de teste.",
                null,
                status,
                DateTime.UtcNow,
                status is StatusOrdemDeServico.AguardandoAprovacao or StatusOrdemDeServico.EmExecucao or StatusOrdemDeServico.Finalizada or StatusOrdemDeServico.Entregue
                    ? DateTime.UtcNow
                    : null,
                status is StatusOrdemDeServico.Finalizada or StatusOrdemDeServico.Entregue ? DateTime.UtcNow : null,
                status == StatusOrdemDeServico.Entregue ? DateTime.UtcNow : null,
                status == StatusOrdemDeServico.Entregue ? DateTime.UtcNow : null,
                0)
            .WithId(id);
    }
}
