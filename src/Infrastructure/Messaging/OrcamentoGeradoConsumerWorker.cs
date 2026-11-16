using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class OrcamentoGeradoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<OrcamentoGeradoConsumerWorker> logger)
    : SqsConsumerWorker<OrcamentoGeradoMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.OrcamentoGeradoQueueUrl;

    protected override async Task ProcessarEventoAsync(
        OrcamentoGeradoMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(evento.Status, "Gerado", StringComparison.Ordinal))
            throw new InvalidOperationException($"Status de orcamento invalido: {evento.Status}.");

        var saga = await context.SagasInicioDiagnostico.FirstOrDefaultAsync(
            item => item.OrdemServicoId == evento.OrdemServicoId &&
                    item.DiagnosticoId == evento.DiagnosticoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga do orcamento gerado nao encontrada.");

        if (evento.CorrelationId != saga.CorrelationId)
            throw new InvalidOperationException("CorrelationId do orcamento nao corresponde a Saga.");

        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ConfirmarOrcamentoGeradoCommand(
            evento.OrdemServicoId,
            evento.DiagnosticoId,
            evento.OrcamentoId,
            evento.ValorTotal,
            DateTimeHelper.UTCToBrazil(evento.OcorridoEm.UtcDateTime)), cancellationToken);
        saga.ConfirmarOrcamento(evento.OrcamentoId, evento.OcorridoEm);
    }
}
