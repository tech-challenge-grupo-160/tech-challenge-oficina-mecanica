namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

/// <summary>
/// Passo repetivel da saga (ADR-0004, F4-27): depois de um pagamento recusado,
/// abre a proxima tentativa e envia ao Billing o comando de cobranca pelo
/// outbox, na mesma transacao.
/// </summary>
public interface ISagaPagamentoService
{
    /// <summary>Devolve o numero da tentativa que passa a valer.</summary>
    Task<int> SolicitarNovaTentativaAsync(int ordemServicoId, CancellationToken cancellationToken);
}
