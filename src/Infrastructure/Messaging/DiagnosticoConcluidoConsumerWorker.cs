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
public sealed class DiagnosticoConcluidoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<DiagnosticoConcluidoConsumerWorker> logger)
    : SqsConsumerWorker<DiagnosticoConcluidoMessage>(scopeFactory, sqs, logger)
{
    private const string BillingDestination = "BillingGerarOrcamento";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
            evento.DiagnosticoId,
            evento.Servicos.Select(item => new ServicoDiagnosticoConcluidoCommand(
                item.ServicoId,
                item.Preco,
                item.TempoEstimado)).ToArray(),
            evento.Pecas.Select(item => new PecaDiagnosticoConcluidoCommand(
                item.PecaId,
                item.Quantidade,
                item.Preco)).ToArray()), cancellationToken);

        var comandoJaCriado = await context.OutboxMessages.AnyAsync(
            item => item.CorrelationId == saga.CorrelationId &&
                    item.Tipo == nameof(GerarOrcamentoMessage) &&
                    item.Destino == BillingDestination,
            cancellationToken);
        if (comandoJaCriado)
            return;

        var comando = new GerarOrcamentoMessage(
            Guid.NewGuid(),
            saga.CorrelationId,
            DateTimeOffset.UtcNow,
            evento.OrdemServicoId,
            evento.DiagnosticoId,
            evento.ParecerTecnico,
            evento.Servicos.Select(item => new ServicoGerarOrcamentoMessage(
                item.ServicoId,
                item.Nome,
                item.Preco,
                item.TempoEstimado)).ToArray(),
            evento.Pecas.Select(item => new PecaGerarOrcamentoMessage(
                item.PecaId,
                item.Nome,
                item.Quantidade,
                item.Preco)).ToArray(),
            evento.ConcluidoEm);

        context.OutboxMessages.Add(OutboxMessage.Criar(
            comando.MessageId,
            comando.CorrelationId,
            nameof(GerarOrcamentoMessage),
            BillingDestination,
            JsonSerializer.Serialize(comando, JsonOptions),
            comando.OcorridoEm));
    }
}
