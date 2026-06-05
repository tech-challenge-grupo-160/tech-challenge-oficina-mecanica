using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public class ClienteApplicationService : IClienteApplicationService
{
    private const string LoggerName = nameof(ClienteApplicationService);
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public ClienteApplicationService(
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IOrdemDeServicoRepository ordemDeServicoRepository,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _clock = clock;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ClienteResult> CriarClienteAsync(CriarClienteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarClienteAsync), "Validando documento e telefone do cliente");
            var documento = Documento.Parse(command.CpfCnpj);
            var telefone = Telefone.Parse(command.Telefone);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarClienteAsync), "Verificando duplicidade de cliente por documento");
            var clienteExistente = await _clienteRepository.ObterPorCpfCnpjAsync(documento.Valor, cancellationToken);
            if (clienteExistente != null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarClienteAsync), "Cliente ja cadastrado para o documento informado");
                throw new ServiceValidationException("Cliente com este CPF/CNPJ ja existe.");
            }

            var cliente = Cliente.Criar(command.Nome, documento, telefone, command.Email, _clock.Now);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarClienteAsync), "Persistindo novo cliente");
            var clienteCriado = await _clienteRepository.CriarAsync(cliente, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente criado com sucesso.");
            return clienteCriado.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarClienteAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<ClienteResult> ObterClienteAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new ServiceNotFoundException($"Cliente com ID {id} nao encontrado.");
        }

        return cliente.ToResult();
    }

    public async Task<ClienteResult> ObterClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ObterClientePorCpfCnpjAsync), "Normalizando documento para consulta");
            var documento = Documento.Parse(cpfCnpj).Valor;

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ObterClientePorCpfCnpjAsync), "Consultando cliente por documento");
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterClientePorCpfCnpjAsync), "Cliente nao encontrado para o documento informado");
                throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente obtido com sucesso.");
            return cliente.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ObterClientePorCpfCnpjAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<PagedResultDto<ClienteResult>> ListarClientesAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken)
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
            return new PagedResultDto<ClienteResult>
            {
                Items = clientes.Select(cliente => cliente.ToResult()).ToArray(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ListarClientesAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<ClienteResult> AtualizarClientePorCpfCnpjAsync(string cpfCnpj, AtualizarClienteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), "Normalizando documento e consultando cliente");
            var documento = Documento.Parse(cpfCnpj).Valor;
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), "Cliente nao encontrado para atualizacao");
                throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
            }

            cliente.AtualizarContato(command.Nome, Telefone.Parse(command.Telefone), command.Email);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), "Persistindo atualizacao do cliente");
            var clienteAtualizado = await _clienteRepository.AtualizarAsync(cliente, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente atualizado com sucesso.");
            return clienteAtualizado.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AtualizarClientePorCpfCnpjAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task DeletarClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Normalizando documento e consultando cliente");
            var documento = Documento.Parse(cpfCnpj).Valor;
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Cliente nao encontrado para exclusao");
                throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Validando dependencias do cliente");
            if (await _veiculoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Cliente possui veiculos vinculados");
                throw new ServiceValidationException("Nao e possivel excluir o cliente, pois existem veiculos vinculados.");
            }

            if (await _ordemDeServicoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Cliente possui ordens de servico vinculadas");
                throw new ServiceValidationException("Nao e possivel excluir o cliente, pois existem ordens de servico vinculadas.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), "Excluindo cliente");
            await _clienteRepository.DeletarAsync(cliente.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente excluido com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(DeletarClientePorCpfCnpjAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
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
