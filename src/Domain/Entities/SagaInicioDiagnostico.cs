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

    public void MarcarFinalizada(DateTimeOffset atualizadaEm)
    {
        Status = "Finalizada";
        AtualizadaEm = atualizadaEm;
    }

    public void Cancelar(DateTimeOffset atualizadaEm)
    {
        Status = "Cancelada";
        AtualizadaEm = atualizadaEm;
    }
}
