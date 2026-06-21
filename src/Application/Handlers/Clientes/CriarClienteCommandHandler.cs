using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Clientes;

public sealed class CriarClienteCommandHandler : IRequestHandler<CriarClienteCommand, ClienteResult>
{
    private const string LoggerName = nameof(CriarClienteCommandHandler);
    private readonly IClienteRepository _clienteRepository;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public CriarClienteCommandHandler(
        IClienteRepository clienteRepository,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        _clienteRepository = clienteRepository;
        _clock = clock;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ClienteResult> Handle(CriarClienteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Validando documento e telefone do cliente");
            var documento = Documento.Parse(command.CpfCnpj);
            var telefone = Telefone.Parse(command.Telefone);
            var email = Email.Parse(command.Email);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Verificando duplicidade de cliente por documento");
            var clienteExistente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (clienteExistente != null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Cliente ja cadastrado para o documento informado");
                throw new ServiceValidationException("Cliente com este CPF/CNPJ ja existe.");
            }

            var cliente = Cliente.Criar(command.Nome, documento, telefone, email, _clock.Now);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Persistindo novo cliente");
            var clienteCriado = await _clienteRepository.CriarAsync(cliente, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente criado com sucesso.");
            return clienteCriado.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
