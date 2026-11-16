namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public sealed class SagaInicioDiagnostico
{
    private SagaInicioDiagnostico() { }

    public Guid CorrelationId { get; private set; }
    public int OrdemServicoId { get; private set; }
    public string Status { get; private set; } = null!;
    public int? DiagnosticoId { get; private set; }
    public Guid? OrcamentoId { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset AtualizadaEm { get; private set; }

    /// <summary>
    /// Cobranca em curso. Comeca em 1; cada nova tentativa depois de uma recusa
    /// soma 1 (F4-27). O Billing deriva dela o Id do pagamento.
    /// </summary>
    public int TentativaPagamento { get; private set; } = 1;

    public static SagaInicioDiagnostico Criar(
        Guid correlationId,
        int ordemServicoId,
        DateTimeOffset criadaEm) => new()
        {
            CorrelationId = correlationId,
            OrdemServicoId = ordemServicoId,
            Status = "Pendente",
            CriadaEm = criadaEm,
            AtualizadaEm = criadaEm
        };

    public void Concluir(int diagnosticoId, DateTimeOffset atualizadaEm)
    {
        DiagnosticoId = diagnosticoId;
        Status = "Concluida";
        AtualizadaEm = atualizadaEm;
    }

    public void ConfirmarOrcamento(Guid orcamentoId, DateTimeOffset atualizadaEm)
    {
        if (orcamentoId == Guid.Empty)
            throw new ArgumentException("OrcamentoId e obrigatorio.", nameof(orcamentoId));
        OrcamentoId = orcamentoId;
        Status = "AguardandoAprovacao";
        AtualizadaEm = atualizadaEm;
    }

    public void MarcarReservandoPecas(DateTimeOffset atualizadaEm)
    {
        Status = "ReservandoPecas";
        AtualizadaEm = atualizadaEm;
    }

    public void MarcarEmExecucao(DateTimeOffset atualizadaEm)
    {
        Status = "EmExecucao";
        AtualizadaEm = atualizadaEm;
    }

    public void MarcarAguardandoEstoque(DateTimeOffset atualizadaEm)
    {
        Status = "AguardandoEstoque";
        AtualizadaEm = atualizadaEm;
    }

    public void MarcarAguardandoPagamento(DateTimeOffset atualizadaEm)
    {
        Status = "AguardandoPagamento";
        AtualizadaEm = atualizadaEm;
    }

    /// <summary>
    /// Compensacao do passo repetivel (ADR-0004): o pagamento foi recusado e a
    /// saga espera uma nova tentativa, sem desfazer a execucao. Devolve false
    /// para uma recusa de tentativa ja superada.
    /// </summary>
    public bool MarcarPagamentoRecusado(int tentativa, DateTimeOffset atualizadaEm)
    {
        if (tentativa < TentativaPagamento)
            return false;
        if (Status == "PagamentoRecusado" && tentativa == TentativaPagamento)
            return false;
        if (Status != "AguardandoPagamento" || tentativa != TentativaPagamento)
            throw new InvalidOperationException(
                $"Recusa da tentativa {tentativa} nao e valida no status {Status} (tentativa atual {TentativaPagamento}).");

        Status = "PagamentoRecusado";
        AtualizadaEm = atualizadaEm;
        return true;
    }

    public void IniciarNovaTentativaPagamento(DateTimeOffset atualizadaEm)
    {
        if (Status != "PagamentoRecusado")
            throw new InvalidOperationException(
                $"Nova cobranca so e possivel depois de um pagamento recusado (status atual: {Status}).");

        TentativaPagamento++;
        Status = "AguardandoPagamento";
        AtualizadaEm = atualizadaEm;
    }

    public void MarcarAguardandoEntrega(DateTimeOffset atualizadaEm)
    {
        Status = "AguardandoEntrega";
        AtualizadaEm = atualizadaEm;
    }

    public void Cancelar(DateTimeOffset atualizadaEm)
    {
        Status = "Cancelada";
        AtualizadaEm = atualizadaEm;
    }
}
