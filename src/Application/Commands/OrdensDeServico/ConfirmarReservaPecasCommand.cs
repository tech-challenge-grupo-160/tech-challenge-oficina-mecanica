using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarReservaPecasCommand(
    int OrdemServicoId,
    bool ReservaCompleta,
    string? DescricaoFaltas)
    : IRequest<Unit>;
