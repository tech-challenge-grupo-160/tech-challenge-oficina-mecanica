namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public bool Enabled { get; init; }
    public string Region { get; init; } = "us-east-1";
    public string? ServiceUrl { get; init; }
    public string ExecucaoCommandQueueUrl { get; init; } = string.Empty;
    public string BillingCommandQueueUrl { get; init; } = string.Empty;
    public string DiagnosticoIniciadoQueueUrl { get; init; } = string.Empty;
    public string DiagnosticoConcluidoQueueUrl { get; init; } = string.Empty;
    public string OrcamentoGeradoQueueUrl { get; init; } = string.Empty;
    public string OrcamentoAprovadoQueueUrl { get; init; } = string.Empty;
    public string OrcamentoRecusadoQueueUrl { get; init; } = string.Empty;
    public string ReservaPecasCommandQueueUrl { get; init; } = string.Empty;
    public string PecasReservadasQueueUrl { get; init; } = string.Empty;
    public string EstoqueInsuficienteQueueUrl { get; init; } = string.Empty;
    public string IniciarExecucaoCommandQueueUrl { get; init; } = string.Empty;
    public string ExecucaoFinalizadaQueueUrl { get; init; } = string.Empty;
}
