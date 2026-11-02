namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    public Guid Id { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string Tipo { get; private set; } = null!;
    public string Destino { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset? PublicadoEm { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }

    public static OutboxMessage Criar(
        Guid id,
        Guid correlationId,
        string tipo,
        string destino,
        string payload,
        DateTimeOffset criadoEm) => new()
        {
            Id = id,
            CorrelationId = correlationId,
            Tipo = tipo,
            Destino = destino,
            Payload = payload,
            CriadoEm = criadoEm
        };

    public void MarcarPublicada(DateTimeOffset publicadoEm)
    {
        PublicadoEm = publicadoEm;
        UltimoErro = null;
    }

    public void RegistrarFalha(string erro)
    {
        Tentativas++;
        UltimoErro = erro.Length <= 2000 ? erro : erro[..2000];
    }
}
