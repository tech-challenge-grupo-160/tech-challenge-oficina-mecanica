using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.AcompanhamentoOS;

public sealed class ObterAcompanhamentoOSQueryHandler : IRequestHandler<ObterAcompanhamentoOSQuery, AcompanhamentoOrdemDeServicoResult>
{
    private const string LoggerName = nameof(ObterAcompanhamentoOSQueryHandler);
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly ILogger _logger;

    public ObterAcompanhamentoOSQueryHandler(
        IOrdemDeServicoRepository ordemRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        ILoggerFactory loggerFactory)
    {
        _ordemRepository = ordemRepository;
        _historicoRepository = historicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<AcompanhamentoOrdemDeServicoResult> Handle(ObterAcompanhamentoOSQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var codigoNormalizado = query.Codigo.Trim().ToUpperInvariant();
            var ordem = await _ordemRepository.ObterPorCodigoAcompanhamentoAsync(codigoNormalizado, cancellationToken);
            if (ordem is null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Codigo de acompanhamento nao encontrado");
                throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
            }

            var tokenHash = StringHelper.ToSha256Hash(query.Token.Trim());
            if (!StringHelper.FixedTimeEqualsHex(tokenHash, ordem.TokenAcompanhamentoHash))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Token de acompanhamento invalido");
                throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
            }

            var historicos = await _historicoRepository.ObterPorOrdemDeServicoAsync(ordem.Id, cancellationToken);
            var ultimaMudancaStatus = historicos
                .Where(h => h.StatusNovo.HasValue && h.StatusAnterior != h.StatusNovo)
                .LastOrDefault();
            var dataUltimaAtualizacao = ultimaMudancaStatus?.DataEvento ?? ordem.DataAbertura;

            var acompanhamento = ordem.ToAcompanhamentoResult(dataUltimaAtualizacao);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Acompanhamento consultado com sucesso para a ordem {ordem.Numero}");
            return acompanhamento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
