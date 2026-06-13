using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class ObterVeiculoPorIdQueryHandler : IRequestHandler<ObterVeiculoPorIdQuery, VeiculoResult>
{
    private readonly IVeiculoRepository _veiculoRepository;

    public ObterVeiculoPorIdQueryHandler(IVeiculoRepository veiculoRepository)
    {
        _veiculoRepository = veiculoRepository;
    }

    public async Task<VeiculoResult> Handle(ObterVeiculoPorIdQuery query, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(query.Id, cancellationToken);
        if (veiculo == null)
        {
            throw new ServiceNotFoundException($"Veiculo com ID {query.Id} nao encontrado.");
        }

        return veiculo.ToResult();
    }
}
