using System.Diagnostics.CodeAnalysis;
using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

/// <summary>
/// Abre a nova tentativa de cobranca (F4-27): a transicao da saga e o comando
/// ao Billing no outbox sao gravados juntos. A regra fica em
/// <see cref="Domain.Entities.SagaInicioDiagnostico.IniciarNovaTentativaPagamento"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SagaPagamentoService(OficinaDbContext context) : ISagaPagamentoService
{
    public async Task<int> SolicitarNovaTentativaAsync(int ordemServicoId, CancellationToken cancellationToken)
    {
        var saga = await context.SagasInicioDiagnostico.SingleOrDefaultAsync(
            item => item.OrdemServicoId == ordemServicoId,
            cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Saga da ordem de servico {ordemServicoId} nao encontrada.");

        var agora = DateTimeOffset.UtcNow;
        saga.IniciarNovaTentativaPagamento(agora);
        PagamentoOutboxFactory.Adicionar(context, saga, agora);
        await context.SaveChangesAsync(cancellationToken);
        return saga.TentativaPagamento;
    }
}
