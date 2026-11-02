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
public sealed class DiagnosticoConcluidoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<DiagnosticoConcluidoConsumerWorker> logger)
    : SqsConsumerWorker<DiagnosticoConcluidoMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.DiagnosticoConcluidoQueueUrl;

    protected override async Task ProcessarEventoAsync(
        DiagnosticoConcluidoMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        var saga = await context.SagasInicioDiagnostico.FirstOrDefaultAsync(
            item => item.OrdemServicoId == evento.OrdemServicoId &&
                    item.DiagnosticoId == evento.DiagnosticoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga do diagnostico concluido nao encontrada.");

        if (evento.CorrelationId != Guid.Empty && saga.CorrelationId != evento.CorrelationId)
            throw new InvalidOperationException("CorrelationId do diagnostico nao corresponde a Saga.");

        if (!string.Equals(saga.Status, "Concluida", StringComparison.Ordinal))
            throw new InvalidOperationException("A saga de inicio do diagnostico ainda nao foi concluida.");

        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ConfirmarDiagnosticoConcluidoCommand(
            evento.OrdemServicoId,
            evento.DiagnosticoId), cancellationToken);
    }
}
