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
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class ExecucaoFinalizadaConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<ExecucaoFinalizadaConsumerWorker> logger)
    : SqsConsumerWorker<ExecucaoFinalizadaMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.ExecucaoFinalizadaQueueUrl;

    protected override async Task ProcessarEventoAsync(
        ExecucaoFinalizadaMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        if (evento.Status != "Finalizada")
            throw new InvalidOperationException($"Status de execucao invalido: {evento.Status}.");

        var saga = await context.SagasInicioDiagnostico.SingleOrDefaultAsync(
            item => item.OrdemServicoId == evento.OrdemServicoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga da execucao nao encontrada.");
        if (saga.CorrelationId != evento.CorrelationId ||
            saga.DiagnosticoId != evento.DiagnosticoId ||
            saga.OrcamentoId != evento.OrcamentoId)
            throw new InvalidOperationException("Execucao finalizada nao corresponde a Saga.");

        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new FinalizarOrdemDeServicoCommand
        {
            Id = evento.OrdemServicoId,
            DataFinalizacao = DateTimeHelper.UTCToBrazil(evento.FinalizadaEm.UtcDateTime)
        }, cancellationToken);
        saga.MarcarFinalizada(evento.FinalizadaEm);
    }
}
