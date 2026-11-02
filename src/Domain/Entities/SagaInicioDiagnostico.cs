namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public sealed class SagaInicioDiagnostico
{
    private SagaInicioDiagnostico() { }

    public Guid CorrelationId { get; private set; }
    public int OrdemServicoId { get; private set; }
    public string Status { get; private set; } = null!;
    public int? DiagnosticoId { get; private set; }
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
}
