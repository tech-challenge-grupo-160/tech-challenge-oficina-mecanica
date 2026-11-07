using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarOrcamentoRecusadoCommand(
    int OrdemServicoId,
    string Motivo)
    : IRequest<Unit>;
