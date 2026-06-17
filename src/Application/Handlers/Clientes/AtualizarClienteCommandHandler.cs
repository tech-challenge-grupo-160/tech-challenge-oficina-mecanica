using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Clientes;

public sealed class AtualizarClienteCommandHandler : IRequestHandler<AtualizarClienteCommand, ClienteResult>
{
    private const string LoggerName = nameof(AtualizarClienteCommandHandler);
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger _logger;

    public AtualizarClienteCommandHandler(
        IClienteRepository clienteRepository,
        ILoggerFactory loggerFactory)
    {
        _clienteRepository = clienteRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ClienteResult> Handle(AtualizarClienteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Normalizando documento e consultando cliente");
            var documento = Documento.Parse(command.CpfCnpj).Valor;
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Cliente nao encontrado para atualizacao");
                throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {command.CpfCnpj} nao encontrado.");
            }

            cliente.AtualizarContato(command.Nome, Telefone.Parse(command.Telefone), command.Email);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Persistindo atualizacao do cliente");
            var clienteAtualizado = await _clienteRepository.AtualizarAsync(cliente, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente atualizado com sucesso.");
            return clienteAtualizado.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
