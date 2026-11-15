using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarDiagnosticoIniciadoCommand(
    int OrdemServicoId,
    int DiagnosticoId) : IRequest<Unit>;
