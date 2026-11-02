using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

public sealed class DiagnosticoIniciadoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<DiagnosticoIniciadoConsumerWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = options.Value.DiagnosticoIniciadoQueueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10,
                    VisibilityTimeout = 30
                }, stoppingToken);

                foreach (var message in response.Messages ?? [])
                    await ProcessarAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha ao consumir eventos de diagnostico iniciado.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task ProcessarAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<DiagnosticoIniciadoMessage>(message.Body, JsonOptions)
                ?? throw new InvalidOperationException("Evento DiagnosticoIniciado invalido.");
            if (evento.Versao != 1)
                throw new InvalidOperationException($"Versao de evento nao suportada: {evento.Versao}.");

            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            if (await context.InboxMessages.AnyAsync(
                    item => item.MessageId == evento.MessageId,
                    cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                await ExcluirAsync(message, cancellationToken);
                return;
            }

            var saga = await context.SagasInicioDiagnostico.FirstOrDefaultAsync(
                item => item.CorrelationId == evento.CorrelationId &&
                        item.OrdemServicoId == evento.OrdemServicoId,
                cancellationToken)
                ?? throw new InvalidOperationException("Saga do inicio do diagnostico nao encontrada.");

            context.InboxMessages.Add(InboxMessage.Criar(
                evento.MessageId,
                evento.CorrelationId,
                nameof(DiagnosticoIniciadoMessage),
                DateTimeOffset.UtcNow));

            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            await mediator.Send(new ConfirmarDiagnosticoIniciadoCommand(
                evento.OrdemServicoId,
                evento.DiagnosticoId), cancellationToken);

            saga.Concluir(evento.DiagnosticoId, DateTimeOffset.UtcNow);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await ExcluirAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao processar mensagem SQS {SqsMessageId}.", message.MessageId);
        }
    }

    private Task<DeleteMessageResponse> ExcluirAsync(
        Message message,
        CancellationToken cancellationToken) =>
        sqs.DeleteMessageAsync(
            options.Value.DiagnosticoIniciadoQueueUrl,
            message.ReceiptHandle,
            cancellationToken);
}
