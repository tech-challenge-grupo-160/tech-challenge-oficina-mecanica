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
public sealed class PagamentoConfirmadoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<PagamentoConfirmadoConsumerWorker> logger)
    : SqsConsumerWorker<PagamentoConfirmadoMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.PagamentoConfirmadoQueueUrl;

    protected override async Task ProcessarEventoAsync(
        PagamentoConfirmadoMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken)
    {
        if (evento.Status != "Confirmado")
            throw new InvalidOperationException($"Status de pagamento invalido: {evento.Status}.");

        var saga = await context.SagasInicioDiagnostico.SingleOrDefaultAsync(
            item => item.OrdemServicoId == evento.OrdemServicoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Saga do pagamento nao encontrada.");
        if (saga.CorrelationId != evento.CorrelationId || saga.OrcamentoId != evento.OrcamentoId)
            throw new InvalidOperationException("Pagamento confirmado nao corresponde a Saga.");

        var ordem = await context.OrdensDeServico.SingleOrDefaultAsync(
            item => item.Id == evento.OrdemServicoId,
            cancellationToken)
            ?? throw new InvalidOperationException("Ordem de servico do pagamento nao encontrada.");
        if (ordem.ValorTotal != evento.Valor)
            throw new InvalidOperationException("Valor confirmado nao corresponde ao total da ordem de servico.");

        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new RegistrarPagamentoCommand
        {
            Id = evento.OrdemServicoId,
            DataPagamento = DateTimeHelper.UTCToBrazil(evento.ConfirmadoEm.UtcDateTime)
        }, cancellationToken);
        saga.MarcarAguardandoEntrega(evento.ConfirmadoEm);
    }
}
