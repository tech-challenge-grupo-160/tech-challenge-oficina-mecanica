using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;

public sealed class ObterClientePorDocumentoQuery : IRequest<ClienteResult>
{
    public string CpfCnpj { get; init; } = null!;
}
