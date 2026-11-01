using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

internal sealed class DiagnosticoExecucaoLocal : IDiagnosticoExecucao
{
    public Task<DiagnosticoExecucao> IniciarAsync(
        int ordemServicoId,
        int veiculoId,
        string descricaoSolicitacao,
        IEnumerable<int> servicoIdsSolicitados,
        IEnumerable<int> pecaIdsSolicitadas,
        CancellationToken cancellationToken) =>
        Task.FromResult(new DiagnosticoExecucao(
            ordemServicoId,
            ordemServicoId,
            "EmDiagnostico",
            DateTimeOffset.UtcNow));
}
