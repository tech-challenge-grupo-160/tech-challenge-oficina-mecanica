using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

public interface IOrdemDeServicoApplicationService
{
    Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServicoHistoricoDto>> ObterHistoricoAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<NotificacaoClienteDto>> ObterNotificacoesAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<MovimentacoesEstoquePorPecaDto>> ObterMovimentacoesEstoqueAsync(int id, CancellationToken cancellationToken);
    Task<MonitoramentoOrdemDeServicoDto> ObterMonitoramentoAsync(int id, CancellationToken cancellationToken);
    Task<EstimativaTempoOrdemDeServicoDto> ObterEstimativaTempoAsync(int id, CancellationToken cancellationToken);
    Task<ResumoMonitoramentoOrdensDeServicoDto> ObterResumoMonitoramentoAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<PagedResultDto<OrdemDeServicoDto>> ListarOrdensDeServicoAsync(
        int page,
        int pageSize,
        int? clienteId,
        string? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> IniciarDiagnosticoAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> FinalizarDiagnosticoAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> AprovarAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> LiberarExecucaoAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> FinalizarAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> RegistrarPagamentoAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> EntregarAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> CancelarAsync(int id, CancelarOrdemDeServicoDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> AtualizarStatusAsync(int id, AtualizarStatusOrdemDeServicoDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> AdicionarServicoAsync(int id, AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> AdicionarPecaAsync(int id, AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> RemoverServicoAsync(int id, int servicoId, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> RemoverPecaAsync(int id, int pecaId, CancellationToken cancellationToken);
}
