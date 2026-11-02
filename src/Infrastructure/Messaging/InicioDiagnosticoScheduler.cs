using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

public sealed class InicioDiagnosticoScheduler(OficinaDbContext context)
    : IInicioDiagnosticoScheduler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid> AgendarAsync(
        int ordemServicoId,
        int veiculoId,
        string descricaoSolicitacao,
        IReadOnlyCollection<int> servicoIdsSolicitados,
        IReadOnlyCollection<int> pecaIdsSolicitadas,
        CancellationToken cancellationToken)
    {
        var existente = await context.SagasInicioDiagnostico
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.OrdemServicoId == ordemServicoId, cancellationToken);
        if (existente is not null)
            return existente.CorrelationId;

        var agora = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var message = new IniciarDiagnosticoMessage(
            Guid.NewGuid(),
            correlationId,
            agora,
            ordemServicoId,
            veiculoId,
            descricaoSolicitacao,
            servicoIdsSolicitados.Order().ToArray(),
            pecaIdsSolicitadas.Distinct().Order().ToArray());

        context.SagasInicioDiagnostico.Add(
            SagaInicioDiagnostico.Criar(correlationId, ordemServicoId, agora));
        context.OutboxMessages.Add(OutboxMessage.Criar(
            message.MessageId,
            correlationId,
            nameof(IniciarDiagnosticoMessage),
            "ExecucaoIniciarDiagnostico",
            JsonSerializer.Serialize(message, JsonOptions),
            agora));

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return correlationId;
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();
            existente = await context.SagasInicioDiagnostico
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.OrdemServicoId == ordemServicoId, cancellationToken);
            if (existente is null)
                throw;
            return existente.CorrelationId;
        }
    }
}
