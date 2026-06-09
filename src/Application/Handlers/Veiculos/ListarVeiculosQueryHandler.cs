using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class ListarVeiculosQueryHandler : IRequestHandler<ListarVeiculosQuery, IEnumerable<VeiculoDto>>
{
    private readonly IVeiculoRepository _veiculoRepository;

    public ListarVeiculosQueryHandler(IVeiculoRepository veiculoRepository)
    {
        _veiculoRepository = veiculoRepository;
    }

    public async Task<IEnumerable<VeiculoDto>> Handle(ListarVeiculosQuery query, CancellationToken cancellationToken)
    {
        var veiculos = await _veiculoRepository.ObterTodosAsync(cancellationToken);
        return veiculos.Select(veiculo => veiculo.ToDto());
    }
}
