using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarDiagnosticoConcluidoCommand(
    int OrdemServicoId,
    int DiagnosticoId) : IRequest<Unit>;
