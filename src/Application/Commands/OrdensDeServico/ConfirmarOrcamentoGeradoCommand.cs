using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarOrcamentoGeradoCommand(
    int OrdemServicoId,
    int DiagnosticoId,
    Guid OrcamentoId,
    decimal ValorTotal,
    DateTime OrcamentoGeradoEm) : IRequest<Unit>;
