using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Pecas;

public sealed class DeletarPecaCommandHandler : IRequestHandler<DeletarPecaCommand, Unit>
{
    private const string LoggerName = nameof(DeletarPecaCommandHandler);
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public DeletarPecaCommandHandler(IPecaRepository pecaRepository, ILoggerFactory loggerFactory)
    {
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<Unit> Handle(DeletarPecaCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando peca para exclusao");
            var peca = await _pecaRepository.ObterPorIdAsync(command.Id, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Peca nao encontrada para exclusao");
                throw new ServiceNotFoundException($"Peca com ID {command.Id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Validando se existem ordens de servico ativas vinculadas a peca");
            if (await _pecaRepository.ExisteEmOrdemDeServicoAtivaAsync(command.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Peca possui ordens de servico ativas vinculadas");
                throw new ServiceValidationException("Nao e possivel excluir a peca pois existem ordens de servico ativas vinculadas.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Excluindo peca");
            await _pecaRepository.DeletarAsync(command.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca excluida com sucesso.");
            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
