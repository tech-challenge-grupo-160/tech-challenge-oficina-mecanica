using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

/// <summary>
/// Compensacao de pagamento recusado (F4-27, ADR-0004). O pagamento e o passo
/// repetivel da saga: a execucao nao e desfeita, a OS segue Finalizada sem
/// pagamento (a entrega fica bloqueada) e a saga espera uma nova cobranca,
/// pedida em POST /ordens-servico/{id}/pagamento/nova-tentativa.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PagamentoRecusadoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<PagamentoRecusadoConsumerWorker> logger)
    : SqsConsumerWorker<PagamentoRecusadoMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.PagamentoRecusadoQueueUrl;

    protected override async Task ProcessarEventoAsync(
        PagamentoRecusadoMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        if (evento.Status != "Recusado")
            throw new InvalidOperationException($"Status de pagamento invalido: {evento.Status}.");

        var saga = await context.SagasInicioDiagnostico.SingleOrDefaultAsync(
            item => item.OrdemServicoId == evento.OrdemServicoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga do pagamento recusado nao encontrada.");
        if (saga.CorrelationId != evento.CorrelationId || saga.OrcamentoId != evento.OrcamentoId)
            throw new InvalidOperationException("Pagamento recusado nao corresponde a Saga.");

        if (!saga.MarcarPagamentoRecusado(evento.Tentativa, evento.RecusadoEm))
        {
            logger.LogWarning(
                "Recusa da tentativa {Tentativa} ignorada: a saga esta em {Status}, tentativa {TentativaAtual}.",
                evento.Tentativa,
                saga.Status,
                saga.TentativaPagamento);
            return;
        }

        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ConfirmarPagamentoRecusadoCommand(
            evento.OrdemServicoId,
            evento.Motivo,
            evento.Tentativa), cancellationToken);
    }
}
