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
public sealed class DiagnosticoIniciadoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<DiagnosticoIniciadoConsumerWorker> logger)
    : SqsConsumerWorker<DiagnosticoIniciadoMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.DiagnosticoIniciadoQueueUrl;

    protected override async Task ProcessarEventoAsync(
        DiagnosticoIniciadoMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        var saga = await context.SagasInicioDiagnostico.FirstOrDefaultAsync(
            item => item.CorrelationId == evento.CorrelationId &&
                    item.OrdemServicoId == evento.OrdemServicoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga do inicio do diagnostico nao encontrada.");

        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ConfirmarDiagnosticoIniciadoCommand(
            evento.OrdemServicoId,
            evento.DiagnosticoId), cancellationToken);

        saga.Concluir(evento.DiagnosticoId, DateTimeOffset.UtcNow);
    }
}
