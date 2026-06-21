using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Clientes;

public sealed class ObterClientePorDocumentoQueryHandler : IRequestHandler<ObterClientePorDocumentoQuery, ClienteResult>
{
    private const string LoggerName = nameof(ObterClientePorDocumentoQueryHandler);
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger _logger;

    public ObterClientePorDocumentoQueryHandler(
        IClienteRepository clienteRepository,
        ILoggerFactory loggerFactory)
    {
        _clienteRepository = clienteRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ClienteResult> Handle(ObterClientePorDocumentoQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Normalizando documento para consulta");
            var documento = Documento.Parse(query.CpfCnpj).Valor;

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando cliente por documento");
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Cliente nao encontrado para o documento informado");
                throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {query.CpfCnpj} nao encontrado.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente obtido com sucesso.");
            return cliente.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
