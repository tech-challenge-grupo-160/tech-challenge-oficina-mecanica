using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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
public sealed class OrcamentoAprovadoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<OrcamentoAprovadoConsumerWorker> logger)
    : SqsConsumerWorker<OrcamentoAprovadoMessage>(scopeFactory, sqs, logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.OrcamentoAprovadoQueueUrl;

    protected override async Task ProcessarEventoAsync(
        OrcamentoAprovadoMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        if (evento.Status != "Aprovado")
            throw new InvalidOperationException($"Status de aprovacao invalido: {evento.Status}.");
        var saga = await ObterSagaAsync(evento, context, cancellationToken);
        var mediator = serviceProvider.GetRequiredService<ISender>();
        var resultado = await mediator.Send(new ConfirmarOrcamentoAprovadoCommand(
            evento.OrdemServicoId,
            evento.ValorTotal), cancellationToken);

        if (!resultado.ReservaNecessaria)
        {
            saga.MarcarEmExecucao(evento.OcorridoEm);
            ExecucaoReparoOutboxFactory.Adicionar(context, saga, evento.OcorridoEm);
            return;
        }

        var comando = new ReservarPecasMessage(
            Guid.NewGuid(),
            evento.CorrelationId,
            DateTimeOffset.UtcNow,
            evento.OrdemServicoId,
            evento.OrcamentoId,
            resultado.Pecas.Select(item =>
                new PecaReservarMessage(item.PecaId, item.Quantidade)).ToArray());
        context.OutboxMessages.Add(OutboxMessage.Criar(
            comando.MessageId,
            comando.CorrelationId,
            nameof(ReservarPecasMessage),
            "ExecucaoReservarPecas",
            JsonSerializer.Serialize(comando, JsonOptions),
            comando.OcorridoEm));
        saga.MarcarReservandoPecas(evento.OcorridoEm);
    }

    private static async Task<SagaInicioDiagnostico> ObterSagaAsync(
        OrcamentoAprovadoMessage evento,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        var saga = await context.SagasInicioDiagnostico.SingleOrDefaultAsync(
            item => item.OrdemServicoId == evento.OrdemServicoId &&
                    item.DiagnosticoId == evento.DiagnosticoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga do orcamento aprovado nao encontrada.");
        if (saga.CorrelationId != evento.CorrelationId || saga.OrcamentoId != evento.OrcamentoId)
            throw new InvalidOperationException("Orcamento aprovado nao corresponde a Saga.");
        return saga;
    }
}
