using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class AtualizarVeiculoCommandHandler : IRequestHandler<AtualizarVeiculoCommand, VeiculoResult>
{
    private readonly IVeiculoRepository _veiculoRepository;

    public AtualizarVeiculoCommandHandler(IVeiculoRepository veiculoRepository)
    {
        _veiculoRepository = veiculoRepository;
    }

    public async Task<VeiculoResult> Handle(AtualizarVeiculoCommand command, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(command.Id, cancellationToken);
        if (veiculo == null)
        {
            throw new ServiceNotFoundException($"Veiculo com ID {command.Id} nao encontrado.");
        }

        veiculo.AtualizarDados(command.Marca, command.Modelo, command.Ano);
        var veiculoAtualizado = await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
        return veiculoAtualizado.ToResult();
    }
}
