using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;

namespace oficina_mecanica.Application.Services;

public interface IVeiculoApplicationService
{
    Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto);
    Task<VeiculoDto> ObterVeiculoAsync(Guid id);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosAsync();
    Task<IEnumerable<VeiculoDto>> ListarVeiculosPorClienteAsync(Guid clienteId);
    Task<VeiculoDto> AtualizarVeiculoAsync(Guid id, AtualizarVeiculoDto dto);
    Task DeletarVeiculoAsync(Guid id);
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

    public async Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(dto.ClienteId);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");
        }

        var veiculoExistente = await _veiculoRepository.ObterPorPlacaAsync(dto.Placa);
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

        var veiculoCriado = await _veiculoRepository.CriarAsync(veiculo);
        return MapToDto(veiculoCriado);
    }

    public async Task<VeiculoDto> ObterVeiculoAsync(Guid id)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");
        }

        return MapToDto(veiculo);
    }

    public async Task<IEnumerable<VeiculoDto>> ListarVeiculosAsync()
    {
        var veiculos = await _veiculoRepository.ObterTodosAsync();
        return veiculos.Select(MapToDto);
    }

    public async Task<IEnumerable<VeiculoDto>> ListarVeiculosPorClienteAsync(Guid clienteId)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {clienteId} não encontrado.");
        }

        var veiculos = await _veiculoRepository.ObterPorClienteAsync(clienteId);
        return veiculos.Select(MapToDto);
    }

    public async Task<VeiculoDto> AtualizarVeiculoAsync(Guid id, AtualizarVeiculoDto dto)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");
        }

        veiculo.Marca = dto.Marca;
        veiculo.Modelo = dto.Modelo;
        veiculo.Ano = dto.Ano;

        var veiculoAtualizado = await _veiculoRepository.AtualizarAsync(veiculo);
        return MapToDto(veiculoAtualizado);
    }

    public async Task DeletarVeiculoAsync(Guid id)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");
        }

        await _veiculoRepository.DeletarAsync(id);
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
