using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
internal static class ExecucaoReparoOutboxFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Adicionar(
        OficinaDbContext context,
        SagaInicioDiagnostico saga,
        DateTimeOffset ocorridoEm)
    {
        if (saga.DiagnosticoId is null || saga.OrcamentoId is null)
            throw new InvalidOperationException("Saga nao possui diagnostico e orcamento para iniciar a execucao.");

        var comando = new IniciarExecucaoMessage(
            Guid.NewGuid(),
            saga.CorrelationId,
            ocorridoEm,
            saga.OrdemServicoId,
            saga.DiagnosticoId.Value,
            saga.OrcamentoId.Value);
        context.OutboxMessages.Add(OutboxMessage.Criar(
            comando.MessageId,
            comando.CorrelationId,
            nameof(IniciarExecucaoMessage),
            "ExecucaoIniciarReparo",
            JsonSerializer.Serialize(comando, JsonOptions),
            ocorridoEm));
    }
}
