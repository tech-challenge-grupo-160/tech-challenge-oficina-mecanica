using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;

public sealed class DeletarClienteCommand : IRequest<Unit>
{
    public string CpfCnpj { get; init; } = null!;
}
