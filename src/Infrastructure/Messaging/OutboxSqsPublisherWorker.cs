using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Amazon.SQS.Model;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class OutboxSqsPublisherWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<OutboxSqsPublisherWorker> logger) : BackgroundService
{
    private const string ExecucaoDestination = "ExecucaoIniciarDiagnostico";
    private const string BillingDestination = "BillingGerarOrcamento";
    private const string ReservaPecasDestination = "ExecucaoReservarPecas";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublicarPendentesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha ao publicar comandos da Outbox no SQS.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task PublicarPendentesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        var mensagens = await context.OutboxMessages
            .Where(item => item.PublicadoEm == null &&
                (item.Destino == ExecucaoDestination ||
                 item.Destino == BillingDestination ||
                 item.Destino == ReservaPecasDestination))
            .OrderBy(item => item.CriadoEm)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var mensagem in mensagens)
        {
            try
            {
                await sqs.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = ObterQueueUrl(mensagem.Destino),
                    MessageBody = mensagem.Payload
                }, cancellationToken);
                mensagem.MarcarPublicada(DateTimeOffset.UtcNow);
            }
            catch (Exception exception)
            {
                mensagem.RegistrarFalha(exception.Message);
                logger.LogWarning(exception, "Falha ao publicar mensagem {MessageId}.", mensagem.Id);
            }
        }

        if (mensagens.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }

    private string ObterQueueUrl(string destino) => destino switch
    {
        ExecucaoDestination => options.Value.ExecucaoCommandQueueUrl,
        BillingDestination => options.Value.BillingCommandQueueUrl,
        ReservaPecasDestination => options.Value.ReservaPecasCommandQueueUrl,
        _ => throw new InvalidOperationException($"Destino de Outbox nao suportado: {destino}.")
    };
}
