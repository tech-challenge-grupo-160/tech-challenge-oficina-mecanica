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

    public OrdemDeServicoHistoricoService(
        IOrdemServicoHistoricoRepository historicoRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        IClock clock)
    {
        _historicoRepository = historicoRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
        _clock = clock;
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

        await _historicoRepository.CriarAsync(
            OrdemServicoHistorico.Registrar(
                ordem.Id,
                usuarioAtual.UsuarioId,
                usuarioAtual.UsuarioNome,
                statusAnterior,
                statusNovo,
                tipoEvento,
                descricao,
                _clock.Now),
            cancellationToken);
    }
}
