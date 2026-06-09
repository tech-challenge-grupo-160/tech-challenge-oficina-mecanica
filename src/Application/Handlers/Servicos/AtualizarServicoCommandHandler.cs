using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Servicos;

public sealed class AtualizarServicoCommandHandler : IRequestHandler<AtualizarServicoCommand, ServicoResult>
{
    private const string LoggerName = nameof(AtualizarServicoCommandHandler);
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public AtualizarServicoCommandHandler(IServicoRepository servicoRepository, ILoggerFactory loggerFactory)
    {
        _servicoRepository = servicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ServicoResult> Handle(AtualizarServicoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando servico para atualizacao");
            var servico = await _servicoRepository.ObterPorIdAsync(command.Id, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Servico nao encontrado para atualizacao");
                throw new ServiceNotFoundException($"Servico com ID {command.Id} nao encontrado.");
            }

            servico.AtualizarDados(command.Nome, command.Descricao, command.Preco, command.TempoEstimado);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Persistindo atualizacao do servico");
            var servicoAtualizado = await _servicoRepository.AtualizarAsync(servico, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico atualizado com sucesso.");
            return servicoAtualizado.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
