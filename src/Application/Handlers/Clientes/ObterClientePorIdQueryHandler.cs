using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Clientes;

public sealed class ObterClientePorIdQueryHandler : IRequestHandler<ObterClientePorIdQuery, ClienteResult>
{
    private readonly IClienteRepository _clienteRepository;

    public ObterClientePorIdQueryHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<ClienteResult> Handle(ObterClientePorIdQuery query, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(query.Id, cancellationToken);
        if (cliente == null)
        {
            throw new ServiceNotFoundException($"Cliente com ID {query.Id} nao encontrado.");
        }

        return cliente.ToResult();
    }
}
