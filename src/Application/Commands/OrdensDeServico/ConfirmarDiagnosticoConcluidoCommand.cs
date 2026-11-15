using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarDiagnosticoConcluidoCommand(
    int OrdemServicoId,
    int DiagnosticoId,
    IReadOnlyCollection<ServicoDiagnosticoConcluidoCommand> Servicos,
    IReadOnlyCollection<PecaDiagnosticoConcluidoCommand> Pecas) : IRequest<Unit>;

public sealed record ServicoDiagnosticoConcluidoCommand(
    int ServicoId,
    decimal Preco,
    int TempoEstimado);

public sealed record PecaDiagnosticoConcluidoCommand(
    int PecaId,
    int Quantidade,
    decimal Preco);
