using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Servicos;

public sealed class DeletarServicoCommandHandler : IRequestHandler<DeletarServicoCommand, Unit>
{
    private const string LoggerName = nameof(DeletarServicoCommandHandler);
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public DeletarServicoCommandHandler(IServicoRepository servicoRepository, ILoggerFactory loggerFactory)
    {
        _servicoRepository = servicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<Unit> Handle(DeletarServicoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando servico para exclusao");
            var servico = await _servicoRepository.ObterPorIdAsync(command.Id, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Servico nao encontrado para exclusao");
                throw new ServiceNotFoundException($"Servico com ID {command.Id} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Validando se existem ordens de servico ativas vinculadas ao servico");
            if (await _servicoRepository.ExisteEmOrdemDeServicoAtivaAsync(command.Id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Servico possui ordens de servico ativas vinculadas");
                throw new ServiceValidationException("Nao e possivel excluir o servico pois existem ordens de servico ativas vinculadas.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Excluindo servico");
            await _servicoRepository.DeletarAsync(command.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico excluido com sucesso.");
            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
