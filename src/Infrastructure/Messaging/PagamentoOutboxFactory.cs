using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
internal static class PagamentoOutboxFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Adicionar(
        OficinaDbContext context,
        SagaInicioDiagnostico saga,
        DateTimeOffset ocorridoEm)
    {
        if (saga.OrcamentoId is null)
            throw new InvalidOperationException("Saga nao possui orcamento para solicitar pagamento.");

        var comando = new SolicitarPagamentoMessage(
            Guid.NewGuid(),
            saga.CorrelationId,
            ocorridoEm,
            saga.OrcamentoId.Value,
            saga.OrdemServicoId,
            Tentativa: saga.TentativaPagamento);
        context.OutboxMessages.Add(OutboxMessage.Criar(
            comando.MessageId,
            comando.CorrelationId,
            nameof(SolicitarPagamentoMessage),
            "BillingSolicitarPagamento",
            JsonSerializer.Serialize(comando, JsonOptions),
            comando.OcorridoEm));
    }
}
