namespace Fiap.TechChallenge.OficinaMecanica.Application.Messaging;

public sealed record IniciarDiagnosticoMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int VeiculoId,
    string DescricaoSolicitacao,
    IReadOnlyCollection<int> ServicoIdsSolicitados,
    IReadOnlyCollection<int> PecaIdsSolicitadas,
    int Versao = 1);

public sealed record DiagnosticoIniciadoMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int DiagnosticoId,
    string Status,
    DateTimeOffset IniciadoEm,
    int Versao = 1);
