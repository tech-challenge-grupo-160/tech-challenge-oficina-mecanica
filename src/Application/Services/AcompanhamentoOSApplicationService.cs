using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IAcompanhamentoOSApplicationService
{
    Task<AcompanhamentoOrdemDeServicoDto> ObterStatusAsync(string codigo, string token, CancellationToken cancellationToken);
}

public sealed class AcompanhamentoOSApplicationService : IAcompanhamentoOSApplicationService
{
    private const string LoggerName = nameof(AcompanhamentoOSApplicationService);
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly ILogger _logger;

    public AcompanhamentoOSApplicationService(
        IOrdemDeServicoRepository ordemRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        ILoggerFactory loggerFactory)
    {
        _ordemRepository = ordemRepository;
        _historicoRepository = historicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<AcompanhamentoOrdemDeServicoDto> ObterStatusAsync(string codigo, string token, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterStatusAsync), "Codigo de acompanhamento nao informado");
                throw new ServiceValidationException("Codigo de acompanhamento e obrigatorio.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterStatusAsync), "Token de acompanhamento nao informado");
                throw new ServiceValidationException("Token de acompanhamento e obrigatorio.");
            }

            var codigoNormalizado = codigo.Trim().ToUpperInvariant();
            var ordem = await _ordemRepository.ObterPorCodigoAcompanhamentoAsync(codigoNormalizado, cancellationToken);
            if (ordem is null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterStatusAsync), "Codigo de acompanhamento nao encontrado");
                throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
            }

            var tokenHash = StringHelper.ToSha256Hash(token.Trim());
            if (!StringHelper.FixedTimeEqualsHex(tokenHash, ordem.TokenAcompanhamentoHash))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterStatusAsync), "Token de acompanhamento invalido");
                throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
            }

            var historicos = await _historicoRepository.ObterPorOrdemDeServicoAsync(ordem.Id, cancellationToken);
            var ultimaMudancaStatus = historicos
                .Where(h => h.StatusNovo.HasValue && h.StatusAnterior != h.StatusNovo)
                .LastOrDefault();
            var dataUltimaAtualizacao = ultimaMudancaStatus?.DataEvento ?? ordem.DataAbertura;

            var acompanhamento = MapToDto(ordem, dataUltimaAtualizacao);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Acompanhamento consultado com sucesso para a ordem {ordem.Numero}");
            return acompanhamento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ObterStatusAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private static AcompanhamentoOrdemDeServicoDto MapToDto(OrdemDeServico ordem, DateTime dataUltimaAtualizacao)
    {
        return new AcompanhamentoOrdemDeServicoDto
        {
            Numero = ordem.Numero,
            CodigoAcompanhamento = ordem.CodigoAcompanhamento,
            Status = ordem.Status.ToString(),
            DataAbertura = ordem.DataAbertura,
            DataUltimaAtualizacao = dataUltimaAtualizacao,
            OrcamentoEnviadoEm = ordem.OrcamentoEnviadoEm,
            DataFinalizacao = ordem.DataFinalizacao,
            DataPagamento = ordem.DataPagamento,
            DataConclusao = ordem.DataConclusao
        };
    }
}
