using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

public sealed class ListarVeiculosPorDocumentoClienteQuery : IRequest<IEnumerable<VeiculoResult>>
{
    public string CpfCnpj { get; init; } = null!;
}
