using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarPagamentoRecusadoCommand(
    int OrdemServicoId,
    string Motivo,
    int Tentativa)
    : IRequest<Unit>;
