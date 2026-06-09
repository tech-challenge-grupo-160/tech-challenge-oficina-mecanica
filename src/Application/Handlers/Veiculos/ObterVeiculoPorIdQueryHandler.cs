using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class ObterVeiculoPorIdQueryHandler : IRequestHandler<ObterVeiculoPorIdQuery, VeiculoDto>
{
    private readonly IVeiculoRepository _veiculoRepository;

    public ObterVeiculoPorIdQueryHandler(IVeiculoRepository veiculoRepository)
    {
        _veiculoRepository = veiculoRepository;
    }

    public async Task<VeiculoDto> Handle(ObterVeiculoPorIdQuery query, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(query.Id, cancellationToken);
        if (veiculo == null)
        {
            throw new ServiceNotFoundException($"Veiculo com ID {query.Id} nao encontrado.");
        }

        return veiculo.ToDto();
    }
}
