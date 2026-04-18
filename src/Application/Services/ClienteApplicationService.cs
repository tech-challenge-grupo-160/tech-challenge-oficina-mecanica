using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IClienteApplicationService
{
    Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto, CancellationToken cancellationToken);
    Task<ClienteDto> ObterClienteAsync(int id, CancellationToken cancellationToken);
    Task<ClienteDto> ObterClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
    Task<PagedResultDto<ClienteDto>> ListarClientesAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken);
    Task<ClienteDto> AtualizarClientePorCpfCnpjAsync(string cpfCnpj, AtualizarClienteDto dto, CancellationToken cancellationToken);
    Task DeletarClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
}

public class ClienteApplicationService : IClienteApplicationService
{
    private const string LoggerName = nameof(ClienteApplicationService);
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly ILogger _logger;

    public ClienteApplicationService(
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IOrdemDeServicoRepository ordemDeServicoRepository,
        ILoggerFactory loggerFactory)
    {
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarClienteAsync), "Validando documento e telefone do cliente");
            var documento = DocumentoHelper.NormalizarDocumento(dto.CpfCnpj);
            var telefone = TelefoneHelper.Normalizar(dto.Telefone);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarClienteAsync), "Verificando duplicidade de cliente por documento");
            var clienteExistente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (clienteExistente != null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarClienteAsync), "Cliente ja cadastrado para o documento informado");
                throw new InvalidOperationException("Cliente com este CPF/CNPJ ja existe.");
            }

            var cliente = new Cliente
            {
                Nome = dto.Nome,
                CpfCnpj = documento,
                Telefone = telefone,
                Email = dto.Email,
                DataCadastro = DateTimeHelper.UTCBrazilNow()
            };

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarClienteAsync), "Persistindo novo cliente");
            var clienteCriado = await _clienteRepository.CriarAsync(cliente, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente criado com sucesso.");
            return MapToDto(clienteCriado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarClienteAsync), ex.Message);
            throw;
        }
    }

    public async Task<ClienteDto> ObterClienteAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} nao encontrado.");
        }

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> ObterClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ObterClientePorCpfCnpjAsync), "Normalizando documento para consulta");
            var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ObterClientePorCpfCnpjAsync), "Consultando cliente por documento");
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterClientePorCpfCnpjAsync), "Cliente nao encontrado para o documento informado");
                throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente obtido com sucesso.");
            return MapToDto(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ObterClientePorCpfCnpjAsync), ex.Message);
            throw;
        }
    }

    public async Task<PagedResultDto<ClienteDto>> ListarClientesAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ListarClientesAsync), "Normalizando filtros de busca");
            var documentoFiltro = NormalizarDocumentoParaBusca(cpfCnpj);

            var nomeFiltro = string.IsNullOrWhiteSpace(nome)
                ? null
                : nome.Trim();

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ListarClientesAsync), "Consultando clientes paginados");
            var totalItems = await _clienteRepository.ContarAsync(nomeFiltro, documentoFiltro, cancellationToken);
            var clientes = await _clienteRepository.ObterPaginadoAsync(page, pageSize, nomeFiltro, documentoFiltro, cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta paginada concluida. Total de registros: {totalItems}");
            return new PagedResultDto<ClienteDto>
            {
                Items = clientes.Select(MapToDto).ToArray(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ListarClientesAsync), ex.Message);
            throw;
        }
    }

    public async Task<ClienteDto> AtualizarClientePorCpfCnpjAsync(string cpfCnpj, AtualizarClienteDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), "Normalizando documento e consultando cliente");
            var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), "Cliente nao encontrado para atualizacao");
                throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
            }

            cliente.Nome = dto.Nome;
            cliente.Telefone = TelefoneHelper.Normalizar(dto.Telefone);
            cliente.Email = dto.Email;

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), "Persistindo atualizacao do cliente");
            var clienteAtualizado = await _clienteRepository.AtualizarAsync(cliente, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente atualizado com sucesso.");
            return MapToDto(clienteAtualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), ex.Message);
            throw;
        }
    }

    public async Task DeletarClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Normalizando documento e consultando cliente");
            var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Cliente nao encontrado para exclusao");
                throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Validando dependencias do cliente");
            if (await _veiculoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Cliente possui veiculos vinculados");
                throw new InvalidOperationException("Nao e possivel excluir o cliente, pois existem veiculos vinculados.");
            }

            if (await _ordemDeServicoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Cliente possui ordens de servico vinculadas");
                throw new InvalidOperationException("Nao e possivel excluir o cliente, pois existem ordens de servico vinculadas.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Excluindo cliente");
            await _clienteRepository.DeletarAsync(cliente.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente excluido com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), ex.Message);
            throw;
        }
    }

    private static ClienteDto MapToDto(Cliente cliente)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            DataCadastro = cliente.DataCadastro
        };
    }

    private static string? NormalizarDocumentoParaBusca(string? cpfCnpj)
    {
        if (string.IsNullOrWhiteSpace(cpfCnpj))
        {
            return null;
        }

        var normalizado = new string(cpfCnpj
            .Trim()
            .Where(c => char.IsDigit(c) || char.IsLetter(c))
            .Select(char.ToUpperInvariant)
            .ToArray());

        return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
    }
}
