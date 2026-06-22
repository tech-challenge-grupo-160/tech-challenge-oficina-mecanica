using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;

public sealed class OrdemDeServicoNotificacaoService
{
    private readonly INotificacaoClienteRepository _notificacaoClienteRepository;
    private readonly IClock _clock;

    public OrdemDeServicoNotificacaoService(
        INotificacaoClienteRepository notificacaoClienteRepository,
        IClock clock)
    {
        _notificacaoClienteRepository = notificacaoClienteRepository;
        _clock = clock;
    }

    public async Task RegistrarAsync(
        int ordemDeServicoId,
        TipoNotificacaoCliente tipoNotificacao,
        CanalNotificacaoCliente canal,
        string mensagem,
        CancellationToken cancellationToken)
    {
        await _notificacaoClienteRepository.CriarAsync(
            NotificacaoCliente.Criar(
                ordemDeServicoId,
                _clock.Now,
                canal,
                tipoNotificacao,
                mensagem,
                true),
            cancellationToken);
    }
}
