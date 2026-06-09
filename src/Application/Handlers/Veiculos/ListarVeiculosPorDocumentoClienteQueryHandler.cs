using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class ListarVeiculosPorDocumentoClienteQueryHandler : IRequestHandler<ListarVeiculosPorDocumentoClienteQuery, IEnumerable<VeiculoResult>>
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;

    public ListarVeiculosPorDocumentoClienteQueryHandler(
        IVeiculoRepository veiculoRepository,
        IClienteRepository clienteRepository)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<IEnumerable<VeiculoResult>> Handle(ListarVeiculosPorDocumentoClienteQuery query, CancellationToken cancellationToken)
    {
        var documento = Documento.Parse(query.CpfCnpj).Valor;
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {query.CpfCnpj} nao encontrado.");
        }

        var veiculos = await _veiculoRepository.ObterPorClienteAsync(cliente.Id, cancellationToken);
        return veiculos.Select(veiculo => veiculo.ToResult());
    }
}
