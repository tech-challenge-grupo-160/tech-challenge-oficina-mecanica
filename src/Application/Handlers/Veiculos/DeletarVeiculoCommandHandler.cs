using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class DeletarVeiculoCommandHandler : IRequestHandler<DeletarVeiculoCommand, Unit>
{
    private const string LoggerName = nameof(DeletarVeiculoCommandHandler);
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly ILogger _logger;

    public DeletarVeiculoCommandHandler(
        IVeiculoRepository veiculoRepository,
        ILoggerFactory loggerFactory)
    {
        _veiculoRepository = veiculoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<Unit> Handle(DeletarVeiculoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando veiculo para exclusao");
            var veiculo = await _veiculoRepository.ObterPorIdAsync(command.Id, cancellationToken);
            if (veiculo == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Veiculo nao encontrado para exclusao");
                throw new ServiceNotFoundException($"Veiculo com ID {command.Id} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Validando se existem ordens de servico ativas vinculadas ao veiculo");
            if (await _veiculoRepository.ExisteEmOrdemDeServicoAtivaAsync(command.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Veiculo possui ordens de servico ativas vinculadas");
                throw new ServiceValidationException("Nao e possivel excluir o veiculo pois existem ordens de servico ativas vinculadas.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Excluindo veiculo");
            await _veiculoRepository.DeletarAsync(command.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Veiculo excluido com sucesso.");
            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
