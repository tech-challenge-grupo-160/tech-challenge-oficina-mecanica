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

[ExcludeFromCodeCoverage]
public sealed class OrcamentoRecusadoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<OrcamentoRecusadoConsumerWorker> logger)
    : SqsConsumerWorker<OrcamentoRecusadoMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.OrcamentoRecusadoQueueUrl;

    protected override async Task ProcessarEventoAsync(
        OrcamentoRecusadoMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        if (evento.Status != "Recusado")
            throw new InvalidOperationException($"Status de recusa invalido: {evento.Status}.");
        var saga = await context.SagasInicioDiagnostico.SingleOrDefaultAsync(
            item => item.OrdemServicoId == evento.OrdemServicoId &&
                    item.DiagnosticoId == evento.DiagnosticoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga do orcamento recusado nao encontrada.");
        if (saga.CorrelationId != evento.CorrelationId || saga.OrcamentoId != evento.OrcamentoId)
            throw new InvalidOperationException("Orcamento recusado nao corresponde a Saga.");

        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ConfirmarOrcamentoRecusadoCommand(
            evento.OrdemServicoId,
            evento.Motivo), cancellationToken);
        saga.Cancelar(evento.OcorridoEm);
    }
}
