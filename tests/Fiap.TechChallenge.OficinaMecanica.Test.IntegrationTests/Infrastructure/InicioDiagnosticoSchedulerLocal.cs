using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

internal sealed class InicioDiagnosticoSchedulerLocal(ISender sender)
    : IInicioDiagnosticoScheduler
{
    public async Task<Guid> AgendarAsync(
        int ordemServicoId,
        int veiculoId,
        string descricaoSolicitacao,
        IReadOnlyCollection<int> servicoIdsSolicitados,
        IReadOnlyCollection<int> pecaIdsSolicitadas,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ConfirmarDiagnosticoIniciadoCommand(ordemServicoId, ordemServicoId),
            cancellationToken);
        return Guid.NewGuid();
    }
}
