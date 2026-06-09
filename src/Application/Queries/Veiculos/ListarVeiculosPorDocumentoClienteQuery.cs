using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

public sealed class ListarVeiculosPorDocumentoClienteQuery : IRequest<IEnumerable<VeiculoDto>>
{
    public string CpfCnpj { get; init; } = null!;
}
