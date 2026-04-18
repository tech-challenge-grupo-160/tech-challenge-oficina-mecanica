using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

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
    private const string LoggerName = nameof(VeiculoApplicationService);
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger _logger;

    public VeiculoApplicationService(
        IVeiculoRepository veiculoRepository,
        IClienteRepository clienteRepository,
        ILoggerFactory loggerFactory)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarVeiculoAsync), "Obtendo cliente por documento");
            var cliente = await ObterClientePorDocumentoAsync(dto.CpfCnpj, cancellationToken);
            var veiculoCriado = await CriarVeiculoInternoAsync(dto.Placa, dto.Marca, dto.Modelo, dto.Ano, cliente.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Veiculo criado com sucesso.");
            return MapToDto(veiculoCriado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarVeiculoAsync), ex.Message);
            throw;
        }
    }

    public async Task<VeiculoDto> CriarVeiculoParaClienteAsync(string cpfCnpj, CriarVeiculoParaClienteDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarVeiculoParaClienteAsync), "Obtendo cliente por documento");
            var cliente = await ObterClientePorDocumentoAsync(cpfCnpj, cancellationToken);
            var veiculoCriado = await CriarVeiculoInternoAsync(dto.Placa, dto.Marca, dto.Modelo, dto.Ano, cliente.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Veiculo criado para o cliente com sucesso.");
            return MapToDto(veiculoCriado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarVeiculoParaClienteAsync), ex.Message);
            throw;
        }
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
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ObterVeiculoPorPlacaAsync), "Normalizando placa e consultando veiculo");
            var placaNormalizada = PlacaHelper.Normalizar(placa);
            var veiculo = await _veiculoRepository.ObterPorPlacaAsync(placaNormalizada, cancellationToken);
            if (veiculo == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterVeiculoPorPlacaAsync), "Veiculo nao encontrado para a placa informada");
                throw new KeyNotFoundException($"Veiculo com placa {placa} nao encontrado.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Veiculo obtido com sucesso.");
            return MapToDto(veiculo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ObterVeiculoPorPlacaAsync), ex.Message);
            throw;
        }
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
