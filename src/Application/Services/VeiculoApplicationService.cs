using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IVeiculoApplicationService
{
    Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto, CancellationToken cancellationToken);
    Task<VeiculoDto> ObterVeiculoAsync(Guid id, CancellationToken cancellationToken);
    Task<VeiculoDto> ObterVeiculoPorPlacaAsync(string placa, CancellationToken cancellationToken);
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
        var placa = PlacaHelper.Normalizar(dto.Placa);
        var documento = DocumentoHelper.NormalizarDocumento(dto.CpfCnpj);
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com CPF/CNPJ {dto.CpfCnpj} nao encontrado.");
        }

        var veiculoExistente = await _veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
        if (veiculoExistente != null)
        {
            throw new InvalidOperationException("Veiculo com esta placa ja existe.");
        }

        var veiculo = new Veiculo
        {
            Id = Guid.NewGuid(),
            Placa = placa,
            Marca = dto.Marca.Trim(),
            Modelo = dto.Modelo.Trim(),
            Ano = dto.Ano,
            ClienteId = cliente.Id
        };

        var veiculoCriado = await _veiculoRepository.CriarAsync(veiculo, cancellationToken);
        return MapToDto(veiculoCriado);
    }

    public async Task<VeiculoDto> ObterVeiculoAsync(Guid id, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veiculo com ID {id} nao encontrado.");
        }

        return MapToDto(veiculo);
    }

    public async Task<VeiculoDto> ObterVeiculoPorPlacaAsync(string placa, CancellationToken cancellationToken)
    {
        var placaNormalizada = PlacaHelper.Normalizar(placa);
        var veiculo = await _veiculoRepository.ObterPorPlacaAsync(placaNormalizada, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veiculo com placa {placa} nao encontrado.");
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
            throw new KeyNotFoundException($"Cliente com ID {clienteId} nao encontrado.");
        }

        var veiculos = await _veiculoRepository.ObterPorClienteAsync(clienteId, cancellationToken);
        return veiculos.Select(MapToDto);
    }

    public async Task<VeiculoDto> AtualizarVeiculoAsync(Guid id, AtualizarVeiculoDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veiculo com ID {id} nao encontrado.");
        }

        veiculo.Marca = dto.Marca.Trim();
        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.Ano = dto.Ano;

        var veiculoAtualizado = await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
        return MapToDto(veiculoAtualizado);
    }

    public async Task DeletarVeiculoAsync(Guid id, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veiculo com ID {id} nao encontrado.");
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
