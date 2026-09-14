using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.AcompanhamentoOS;

public sealed class ObterAcompanhamentoOSQueryHandler : IRequestHandler<ObterAcompanhamentoOSQuery, AcompanhamentoOrdemDeServicoResult>
{
    private const string LoggerName = nameof(ObterAcompanhamentoOSQueryHandler);
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly IUsuarioAutenticadoService _usuarioAutenticadoService;
    private readonly ILogger _logger;

    public ObterAcompanhamentoOSQueryHandler(
        IOrdemDeServicoRepository ordemRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        ILoggerFactory loggerFactory)
    {
        _ordemRepository = ordemRepository;
        _historicoRepository = historicoRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<AcompanhamentoOrdemDeServicoResult> Handle(ObterAcompanhamentoOSQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var codigoNormalizado = query.CodigoAcompanhamento.Trim().ToUpperInvariant();
            var ordem = await _ordemRepository.ObterPorCodigoAcompanhamentoAsync(codigoNormalizado, cancellationToken);
            if (ordem is null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Codigo de acompanhamento nao encontrado");
                throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
            }

            ValidarClienteAutenticado(ordem);

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

    private void ValidarClienteAutenticado(OrdemDeServico ordem)
    {
        var usuarioAtual = _usuarioAutenticadoService.ObterUsuarioAtual();
        if (string.IsNullOrWhiteSpace(usuarioAtual.ClienteDocumento))
        {
            _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ValidarClienteAutenticado), "Documento do cliente nao encontrado no token JWT");
            throw new ServiceUnauthorizedException("Documento do cliente nao encontrado no token.");
        }

        if (ordem.Cliente?.CpfCnpj.Valor != usuarioAtual.ClienteDocumento)
        {
            _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ValidarClienteAutenticado), "Cliente autenticado nao pertence a ordem consultada");
            throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
        }
    }
}
