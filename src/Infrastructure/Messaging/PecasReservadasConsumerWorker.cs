using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class PecasReservadasConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<PecasReservadasConsumerWorker> logger)
    : SqsConsumerWorker<PecasReservadasMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.PecasReservadasQueueUrl;

    protected override async Task ProcessarEventoAsync(
        PecasReservadasMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        var saga = await ObterSagaAsync(
            evento.OrdemServicoId, evento.CorrelationId, evento.OrcamentoId,
            context, cancellationToken);
        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ConfirmarReservaPecasCommand(
            evento.OrdemServicoId, true, null), cancellationToken);
        saga.MarcarEmExecucao(evento.OcorridoEm);
        ExecucaoReparoOutboxFactory.Adicionar(context, saga, evento.OcorridoEm);
    }

    internal static async Task<SagaInicioDiagnostico> ObterSagaAsync(
        int ordemServicoId,
        Guid correlationId,
        Guid orcamentoId,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        var saga = await context.SagasInicioDiagnostico.SingleOrDefaultAsync(
            item => item.OrdemServicoId == ordemServicoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga da reserva de pecas nao encontrada.");
        if (saga.CorrelationId != correlationId || saga.OrcamentoId != orcamentoId)
            throw new InvalidOperationException("Resultado da reserva nao corresponde a Saga.");
        return saga;
    }
}
