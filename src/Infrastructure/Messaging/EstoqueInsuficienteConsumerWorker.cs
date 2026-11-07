using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class EstoqueInsuficienteConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<EstoqueInsuficienteConsumerWorker> logger)
    : SqsConsumerWorker<EstoqueInsuficienteMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.EstoqueInsuficienteQueueUrl;

    protected override async Task ProcessarEventoAsync(
        EstoqueInsuficienteMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        var saga = await PecasReservadasConsumerWorker.ObterSagaAsync(
            evento.OrdemServicoId, evento.CorrelationId, evento.OrcamentoId,
            context, cancellationToken);
        var faltas = string.Join(", ", evento.Pecas
            .Where(item => item.QuantidadeFaltante > 0)
            .Select(item => $"peca {item.PecaId}: faltam {item.QuantidadeFaltante}"));
        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ConfirmarReservaPecasCommand(
            evento.OrdemServicoId, false, faltas), cancellationToken);
        saga.MarcarAguardandoEstoque(evento.OcorridoEm);
    }
}
