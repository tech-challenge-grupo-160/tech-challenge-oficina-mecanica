using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;

public sealed class ObterClientePorIdQuery : IRequest<ClienteResult>
{
    public int Id { get; init; }
}
