using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Clientes;

public sealed class DeletarClienteCommandHandler : IRequestHandler<DeletarClienteCommand, Unit>
{
    private const string LoggerName = nameof(DeletarClienteCommandHandler);
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly ILogger _logger;

    public DeletarClienteCommandHandler(
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

    public async Task<Unit> Handle(DeletarClienteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Normalizando documento e consultando cliente");
            var documento = Documento.Parse(command.CpfCnpj).Valor;
            var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Cliente nao encontrado para exclusao");
                throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {command.CpfCnpj} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Validando dependencias do cliente");
            if (await _veiculoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Cliente possui veiculos vinculados");
                throw new ServiceValidationException("Nao e possivel excluir o cliente, pois existem veiculos vinculados.");
            }

            if (await _ordemDeServicoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Cliente possui ordens de servico vinculadas");
                throw new ServiceValidationException("Nao e possivel excluir o cliente, pois existem ordens de servico vinculadas.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Excluindo cliente");
            await _clienteRepository.DeletarAsync(cliente.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Cliente excluido com sucesso.");
            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
