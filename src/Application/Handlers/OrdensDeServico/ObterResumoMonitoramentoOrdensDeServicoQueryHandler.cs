using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterResumoMonitoramentoOrdensDeServicoQueryHandler : IRequestHandler<ObterResumoMonitoramentoOrdensDeServicoQuery, ResumoMonitoramentoOrdensDeServicoDto>
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

    public Task<ResumoMonitoramentoOrdensDeServicoDto> Handle(ObterResumoMonitoramentoOrdensDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterResumoMonitoramentoAsync(query.Page, query.PageSize, cancellationToken);
    }

    private async Task<ResumoMonitoramentoOrdensDeServicoDto> ObterResumoMonitoramentoAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var agora = _clock.Now;
        var ordens = (await _ordemRepository.ObterTodasAsync(cancellationToken)).ToList();
        var ordensMonitoradas = ordens
            .Select(ordem => OrdemDeServicoMapper.ToMonitoramentoDto(ordem, agora))
            .ToList();

        var ordensFinalizadas = ordensMonitoradas
            .Where(ordem => ordem.TempoFinalizacaoMinutos.HasValue)
            .ToList();

        var tempoMedioFinalizacaoMinutos = ordensFinalizadas.Count == 0
            ? (int?)null
            : (int)Math.Round(ordensFinalizadas.Average(ordem => ordem.TempoFinalizacaoMinutos!.Value));

        var totalOrdens = ordensMonitoradas.Count;
        var ordensPaginadas = ordensMonitoradas
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new ResumoMonitoramentoOrdensDeServicoDto
        {
            TotalOrdens = totalOrdens,
            TotalOrdensAbertas = ordensMonitoradas.Count(ordem => !ordem.EstaFinalizada),
            TotalOrdensFinalizadas = ordensFinalizadas.Count,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalOrdens == 0 ? 0 : (int)Math.Ceiling(totalOrdens / (double)pageSize),
            TempoMedioFinalizacaoMinutos = tempoMedioFinalizacaoMinutos,
            TempoMedioFinalizacaoHoras = tempoMedioFinalizacaoMinutos.HasValue
                ? Math.Round(tempoMedioFinalizacaoMinutos.Value / 60d, 2)
                : null,
            Ordens = ordensPaginadas
        };
    }
}


