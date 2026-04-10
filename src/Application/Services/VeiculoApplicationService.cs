using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IVeiculoApplicationService
{
    Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto, CancellationToken cancellationToken);
    Task<VeiculoDto> ObterVeiculoAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosAsync(CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosPorClienteAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<VeiculoDto> AtualizarVeiculoAsync(Guid id, AtualizarVeiculoDto dto, CancellationToken cancellationToken);
    Task DeletarVeiculoAsync(Guid id, CancellationToken cancellationToken);
}

public class VeiculoApplicationService : IVeiculoApplicationService
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;

    public VeiculoApplicationService(IVeiculoRepository veiculoRepository, IClienteRepository clienteRepository)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(dto.ClienteId, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");
        }

        var veiculoExistente = await _veiculoRepository.ObterPorPlacaAsync(dto.Placa, cancellationToken);
        if (veiculoExistente != null)
        {
            throw new InvalidOperationException("Veículo com esta placa já existe.");
        }

        var veiculo = new Veiculo
        {
            Id = Guid.NewGuid(),
            Placa = dto.Placa.ToUpper(),
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Ano = dto.Ano,
            ClienteId = dto.ClienteId
        };

        var veiculoCriado = await _veiculoRepository.CriarAsync(veiculo, cancellationToken);
        return MapToDto(veiculoCriado);
    }

    public async Task<VeiculoDto> ObterVeiculoAsync(Guid id, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");
        }

        return MapToDto(veiculo);
    }

    public async Task<IEnumerable<VeiculoDto>> ListarVeiculosAsync(CancellationToken cancellationToken)
    {
        var veiculos = await _veiculoRepository.ObterTodosAsync(cancellationToken);
        return veiculos.Select(MapToDto);
    }

    public async Task<IEnumerable<VeiculoDto>> ListarVeiculosPorClienteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {clienteId} não encontrado.");
        }

        var veiculos = await _veiculoRepository.ObterPorClienteAsync(clienteId, cancellationToken);
        return veiculos.Select(MapToDto);
    }

    public async Task<VeiculoDto> AtualizarVeiculoAsync(Guid id, AtualizarVeiculoDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");
        }

        veiculo.Marca = dto.Marca;
        veiculo.Modelo = dto.Modelo;
        veiculo.Ano = dto.Ano;

        var veiculoAtualizado = await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
        return MapToDto(veiculoAtualizado);
    }

    public async Task DeletarVeiculoAsync(Guid id, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");
        }

        await _veiculoRepository.DeletarAsync(id, cancellationToken);
    }

    private static VeiculoDto MapToDto(Veiculo veiculo)
    {
        return new VeiculoDto
        {
            Id = veiculo.Id,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            ClienteId = veiculo.ClienteId
        };
    }
}
