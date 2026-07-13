using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class ListarVeiculosPorClienteQueryHandler : IRequestHandler<ListarVeiculosPorClienteQuery, IEnumerable<VeiculoResult>>
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;

    public ListarVeiculosPorClienteQueryHandler(
        IVeiculoRepository veiculoRepository,
        IClienteRepository clienteRepository)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<IEnumerable<VeiculoResult>> Handle(ListarVeiculosPorClienteQuery query, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(query.ClienteId, cancellationToken);
        if (cliente == null)
        {
            throw new ServiceNotFoundException($"Cliente com ID {query.ClienteId} nao encontrado.");
        }

        var veiculos = await _veiculoRepository.ObterPorClienteAsync(query.ClienteId, cancellationToken);
        return veiculos.Select(veiculo => veiculo.ToResult());
    }
}
