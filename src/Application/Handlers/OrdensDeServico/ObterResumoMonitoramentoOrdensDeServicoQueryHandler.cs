using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterResumoMonitoramentoOrdensDeServicoQueryHandler : IRequestHandler<ObterResumoMonitoramentoOrdensDeServicoQuery, ResumoMonitoramentoOrdensDeServicoResult>
{
    private const string LoggerName = nameof(ObterResumoMonitoramentoOrdensDeServicoQueryHandler);
    private readonly IClock _clock;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public ObterResumoMonitoramentoOrdensDeServicoQueryHandler(
        IClock clock,
        IOrdemDeServicoRepository ordemRepository,
        ILoggerFactory loggerFactory)
    {
        _clock = clock;
        _ordemRepository = ordemRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<ResumoMonitoramentoOrdensDeServicoResult> Handle(ObterResumoMonitoramentoOrdensDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterResumoMonitoramentoAsync(query.Page, query.PageSize, cancellationToken);
    }

    private async Task<ResumoMonitoramentoOrdensDeServicoResult> ObterResumoMonitoramentoAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var agora = _clock.Now;

        var (total, abertas, finalizadas) = await _ordemRepository.ContarParaMonitoramentoAsync(cancellationToken);
        var tempoMedioFinalizacaoMinutos = await _ordemRepository.ObterTempoMedioFinalizacaoMinutosAsync(cancellationToken);

        var ordensPagina = await _ordemRepository.ObterParaMonitoramentoAsync(page, pageSize, cancellationToken);
        var ordensPaginadas = ordensPagina
            .Select(ordem => OrdemDeServicoMapper.ToMonitoramentoResult(ordem, agora))
            .ToList();

        return new ResumoMonitoramentoOrdensDeServicoResult
        {
            TotalOrdens = total,
            TotalOrdensAbertas = abertas,
            TotalOrdensFinalizadas = finalizadas,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
            TempoMedioFinalizacaoMinutos = tempoMedioFinalizacaoMinutos,
            TempoMedioFinalizacaoHoras = tempoMedioFinalizacaoMinutos.HasValue
                ? Math.Round(tempoMedioFinalizacaoMinutos.Value / 60d, 2)
                : null,
            Ordens = ordensPaginadas
        };
    }
}


