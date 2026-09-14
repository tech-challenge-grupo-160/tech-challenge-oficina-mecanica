using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;

public sealed class OrdemDeServicoHistoricoService
{
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly IUsuarioAutenticadoService _usuarioAutenticadoService;
    private readonly IClock _clock;
    private readonly IBusinessMetrics? _businessMetrics;

    public OrdemDeServicoHistoricoService(
        IOrdemServicoHistoricoRepository historicoRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        IClock clock,
        IBusinessMetrics? businessMetrics = null)
    {
        _historicoRepository = historicoRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
        _clock = clock;
        _businessMetrics = businessMetrics;
    }

    public async Task RegistrarAsync(
        OrdemDeServico ordem,
        TipoEventoOrdemServico tipoEvento,
        StatusOrdemDeServico? statusAnterior,
        StatusOrdemDeServico? statusNovo,
        string descricao,
        CancellationToken cancellationToken)
    {
        var usuarioAtual = _usuarioAutenticadoService.ObterUsuarioAtual();
        var dataEvento = _clock.Now;

        if (statusAnterior.HasValue && statusNovo.HasValue)
        {
            var etapa = GetStage(statusAnterior.Value);
            if (etapa is not null)
            {
                var historico = await _historicoRepository.ObterPorOrdemDeServicoAsync(
                    ordem.Id,
                    cancellationToken);
                var inicioEtapa = historico
                    .Where(item => item.StatusNovo == statusAnterior)
                    .OrderByDescending(item => item.DataEvento)
                    .FirstOrDefault();

                if (inicioEtapa is not null)
                {
                    _businessMetrics?.RecordOrderStageDuration(
                        etapa,
                        (dataEvento - inicioEtapa.DataEvento).TotalSeconds);
                }
            }
        }

        await _historicoRepository.CriarAsync(
            OrdemServicoHistorico.Registrar(
                ordem.Id,
                usuarioAtual.UsuarioId,
                usuarioAtual.UsuarioNome,
                statusAnterior,
                statusNovo,
                tipoEvento,
                descricao,
                dataEvento),
            cancellationToken);
    }

    private static string? GetStage(StatusOrdemDeServico status)
    {
        return status switch
        {
            StatusOrdemDeServico.EmDiagnostico => "diagnostico",
            StatusOrdemDeServico.EmExecucao => "execucao",
            StatusOrdemDeServico.Finalizada => "finalizacao",
            _ => null
        };
    }
}
