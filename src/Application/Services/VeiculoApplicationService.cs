using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IVeiculoApplicationService
{
    Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto, CancellationToken cancellationToken);
    Task<VeiculoDto> CriarVeiculoParaClienteAsync(string cpfCnpj, CriarVeiculoParaClienteDto dto, CancellationToken cancellationToken);
    Task<VeiculoDto> ObterVeiculoAsync(int id, CancellationToken cancellationToken);
    Task<VeiculoDto> ObterVeiculoPorPlacaAsync(string placa, CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosAsync(CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosPorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosPorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
    Task<VeiculoDto> AtualizarVeiculoAsync(int id, AtualizarVeiculoDto dto, CancellationToken cancellationToken);
    Task DeletarVeiculoAsync(int id, CancellationToken cancellationToken);
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
        var cliente = await ObterClientePorDocumentoAsync(dto.CpfCnpj, cancellationToken);
        var veiculoCriado = await CriarVeiculoInternoAsync(dto.Placa, dto.Marca, dto.Modelo, dto.Ano, cliente.Id, cancellationToken);
        return MapToDto(veiculoCriado);
    }

    public async Task<VeiculoDto> CriarVeiculoParaClienteAsync(string cpfCnpj, CriarVeiculoParaClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await ObterClientePorDocumentoAsync(cpfCnpj, cancellationToken);
        var veiculoCriado = await CriarVeiculoInternoAsync(dto.Placa, dto.Marca, dto.Modelo, dto.Ano, cliente.Id, cancellationToken);
        return MapToDto(veiculoCriado);
    }

    public async Task<VeiculoDto> ObterVeiculoAsync(int id, CancellationToken cancellationToken)
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

    public async Task<IEnumerable<VeiculoDto>> ListarVeiculosPorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {clienteId} nao encontrado.");
        }

        var veiculos = await _veiculoRepository.ObterPorClienteAsync(clienteId, cancellationToken);
        return veiculos.Select(MapToDto);
    }

    public async Task<IEnumerable<VeiculoDto>> ListarVeiculosPorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        var cliente = await ObterClientePorDocumentoAsync(cpfCnpj, cancellationToken);
        var veiculos = await _veiculoRepository.ObterPorClienteAsync(cliente.Id, cancellationToken);
        return veiculos.Select(MapToDto);
    }

    public async Task<VeiculoDto> AtualizarVeiculoAsync(int id, AtualizarVeiculoDto dto, CancellationToken cancellationToken)
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

    public async Task DeletarVeiculoAsync(int id, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veiculo com ID {id} nao encontrado.");
        }

        await _veiculoRepository.DeletarAsync(id, cancellationToken);
    }

    private async Task<Cliente> ObterClientePorDocumentoAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
        }

        return cliente;
    }

    private async Task<Veiculo> CriarVeiculoInternoAsync(
        string placaInformada,
        string marca,
        string modelo,
        int ano,
        int clienteId,
        CancellationToken cancellationToken)
    {
        var placa = PlacaHelper.Normalizar(placaInformada);
        var veiculoExistente = await _veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
        if (veiculoExistente != null)
        {
            throw new InvalidOperationException("Veiculo com esta placa ja existe.");
        }

        var veiculo = new Veiculo
        {
            Placa = placa,
            Marca = marca.Trim(),
            Modelo = modelo.Trim(),
            Ano = ano,
            ClienteId = clienteId
        };

        return await _veiculoRepository.CriarAsync(veiculo, cancellationToken);
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
