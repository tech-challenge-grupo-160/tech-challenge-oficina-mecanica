using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public abstract class SqsConsumerWorker<TMessage>(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    ILogger logger) : BackgroundService
    where TMessage : IMensagemIntegracao
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected abstract bool Enabled { get; }
    protected abstract string QueueUrl { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = QueueUrl,
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
                logger.LogError(exception, "Falha ao consumir mensagens {TipoMensagem}.", typeof(TMessage).Name);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task ProcessarAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<TMessage>(message.Body, JsonOptions)
                ?? throw new InvalidOperationException($"Mensagem {typeof(TMessage).Name} invalida.");
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

            context.InboxMessages.Add(InboxMessage.Criar(
                evento.MessageId,
                evento.CorrelationId,
                typeof(TMessage).Name,
                DateTimeOffset.UtcNow));

            await ProcessarEventoAsync(evento, scope.ServiceProvider, context, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await ExcluirAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Falha ao processar mensagem {TipoMensagem} com ID SQS {SqsMessageId}.",
                typeof(TMessage).Name,
                message.MessageId);
        }
    }

    protected abstract Task ProcessarEventoAsync(
        TMessage evento,
        IServiceProvider serviceProvider,
        OficinaDbContext context,
        CancellationToken cancellationToken);

    private Task<DeleteMessageResponse> ExcluirAsync(
        Message message,
        CancellationToken cancellationToken) =>
        sqs.DeleteMessageAsync(QueueUrl, message.ReceiptHandle, cancellationToken);
}
