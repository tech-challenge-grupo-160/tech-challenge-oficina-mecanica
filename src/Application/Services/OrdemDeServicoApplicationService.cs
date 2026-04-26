using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IOrdemDeServicoApplicationService
{
    Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServicoHistoricoDto>> ObterHistoricoAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<NotificacaoClienteDto>> ObterNotificacoesAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<MovimentacaoEstoqueDto>> ObterMovimentacoesEstoqueAsync(int id, CancellationToken cancellationToken);
    Task<MonitoramentoOrdemDeServicoDto> ObterMonitoramentoAsync(int id, CancellationToken cancellationToken);
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

public class OrdemDeServicoApplicationService : IOrdemDeServicoApplicationService
{
    private const string LoggerName = nameof(OrdemDeServicoApplicationService);
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IPedidoCompraRepository _pedidoCompraRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly INotificacaoClienteRepository _notificacaoClienteRepository;
    private readonly IUsuarioAutenticadoService _usuarioAutenticadoService;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;

    public OrdemDeServicoApplicationService(
        IOrdemDeServicoRepository ordemRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IPecaRepository pecaRepository,
        IPedidoCompraRepository pedidoCompraRepository,
        IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        INotificacaoClienteRepository notificacaoClienteRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        ITransactionManager transactionManager,
        ILoggerFactory loggerFactory)
    {
        _ordemRepository = ordemRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _pecaRepository = pecaRepository;
        _pedidoCompraRepository = pedidoCompraRepository;
        _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
        _historicoRepository = historicoRepository;
        _notificacaoClienteRepository = notificacaoClienteRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
        _transactionManager = transactionManager;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Consultando cliente e veiculo informados");
            var cliente = await _clienteRepository.ObterPorIdAsync(dto.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Cliente nao encontrado para abertura da OS");
                throw new ServiceNotFoundException($"Cliente com ID {dto.ClienteId} nao encontrado.");
            }

            var veiculo = await _veiculoRepository.ObterPorIdAsync(dto.VeiculoId, cancellationToken);
            if (veiculo == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao encontrado para abertura da OS");
                throw new ServiceNotFoundException($"Veiculo com ID {dto.VeiculoId} nao encontrado.");
            }

            if (veiculo.ClienteId != dto.ClienteId)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao pertence ao cliente informado");
                throw new ServiceValidationException("O veiculo informado nao pertence ao cliente informado.");
            }

            var existeOrdemAtiva = await _ordemRepository.ExisteOrdemAtivaPorClienteEVeiculoAsync(dto.ClienteId, dto.VeiculoId, cancellationToken);
            if (existeOrdemAtiva)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Ja existe ordem de servico ativa para o cliente e veiculo informados");
                throw new ServiceValidationException("Ja existe uma ordem de servico ativa para este cliente e veiculo.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Persistindo ordem de servico em status Recebida");
            var (codigoAcompanhamento, tokenAcompanhamento, tokenAcompanhamentoHash) =
                await GerarCredenciaisAcompanhamentoAsync(cancellationToken);

            var ordem = new OrdemDeServico
            {
                Numero = GerarNumeroTemporario(),
                CodigoAcompanhamento = codigoAcompanhamento,
                TokenAcompanhamentoHash = tokenAcompanhamentoHash,
                ClienteId = dto.ClienteId,
                VeiculoId = dto.VeiculoId,
                DescricaoSolicitacao = dto.DescricaoSolicitacao.Trim(),
                ObservacoesRecepcao = string.IsNullOrWhiteSpace(dto.ObservacoesRecepcao) ? null : dto.ObservacoesRecepcao.Trim(),
                Status = StatusOrdemDeServico.Recebida,
                DataAbertura = DateTimeHelper.UTCBrazilNow(),
                ValorTotal = 0
            };

            var ordemCriada = await _ordemRepository.CriarAsync(ordem, cancellationToken);
            ordemCriada.Numero = GerarNumeroOrdem(ordemCriada.Id, ordemCriada.DataAbertura);

            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordemCriada, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.OrdemCriada,
                null,
                ordemAtualizada.Status,
                "Ordem de servico criada.",
                cancellationToken);
            await RegistrarNotificacaoClienteAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.LinkAcompanhamentoEnviado,
                CanalNotificacaoCliente.Email,
                $"Link de acompanhamento da ordem {ordemAtualizada.Numero} enviado para o e-mail {cliente.Email}. Endpoint: {MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico aberta com sucesso. Numero: {ordemAtualizada.Numero}");
            var resposta = MapToDto(ordemAtualizada);
            resposta.TokenAcompanhamento = tokenAcompanhamento;
            return resposta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarOrdemDeServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return MapToDto(ordem);
    }

    public async Task<IEnumerable<OrdemServicoHistoricoDto>> ObterHistoricoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var historicos = await _historicoRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return historicos.Select(MapToDto);
    }

    public async Task<IEnumerable<NotificacaoClienteDto>> ObterNotificacoesAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var notificacoes = await _notificacaoClienteRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return notificacoes.Select(MapToDto);
    }

    public async Task<IEnumerable<MovimentacaoEstoqueDto>> ObterMovimentacoesEstoqueAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var movimentacoes = await _movimentacaoEstoqueRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return movimentacoes.Select(MapToDto);
    }

    public async Task<MonitoramentoOrdemDeServicoDto> ObterMonitoramentoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return MapToMonitoramentoDto(ordem, DateTimeHelper.UTCBrazilNow());
    }

    public async Task<ResumoMonitoramentoOrdensDeServicoDto> ObterResumoMonitoramentoAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var agora = DateTimeHelper.UTCBrazilNow();
        var ordens = (await _ordemRepository.ObterTodasAsync(cancellationToken)).ToList();
        var ordensMonitoradas = ordens
            .Select(ordem => MapToMonitoramentoDto(ordem, agora))
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

    public async Task<PagedResultDto<OrdemDeServicoDto>> ListarOrdensDeServicoAsync(
        int page,
        int pageSize,
        int? clienteId,
        string? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken)
    {
        StatusOrdemDeServico? statusFiltro = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusOrdemDeServico>(status, true, out var statusEnum))
            {
                throw new ServiceValidationException($"Status invalido: {status}");
            }

            statusFiltro = statusEnum;
        }

        var numeroFiltro = string.IsNullOrWhiteSpace(numero) ? null : numero.Trim();
        var totalItems = await _ordemRepository.ContarAsync(
            clienteId,
            statusFiltro,
            numeroFiltro,
            dataAberturaInicio,
            dataAberturaFim,
            cancellationToken);

        var ordens = await _ordemRepository.ObterPaginadoAsync(
            page,
            pageSize,
            clienteId,
            statusFiltro,
            numeroFiltro,
            dataAberturaInicio,
            dataAberturaFim,
            cancellationToken);

        return new PagedResultDto<OrdemDeServicoDto>
        {
            Items = ordens.Select(MapToDto).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<OrdemDeServicoDto> AtualizarStatusAsync(int id, AtualizarStatusOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarStatusAsync), "Consultando ordem de servico para alteracao de status");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AtualizarStatusAsync), "Ordem de servico nao encontrada para alteracao de status");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            if (!Enum.TryParse<StatusOrdemDeServico>(dto.NovoStatus, out var novoStatus))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AtualizarStatusAsync), "Status informado e invalido");
                throw new ServiceValidationException($"Status invalido: {dto.NovoStatus}");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarStatusAsync), $"Alterando status da ordem para {novoStatus}");
            ordem.AlterarStatus(novoStatus);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Status da ordem atualizado com sucesso para {ordemAtualizada.Status}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AtualizarStatusAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> IniciarDiagnosticoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(IniciarDiagnosticoAsync), "Consultando ordem de servico para iniciar diagnostico");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(IniciarDiagnosticoAsync), "Ordem de servico nao encontrada para iniciar diagnostico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(IniciarDiagnosticoAsync), "Alterando status da ordem para EmDiagnostico");
            var statusAnterior = ordem.Status;
            ordem.AlterarStatus(StatusOrdemDeServico.EmDiagnostico);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.DiagnosticoIniciado,
                statusAnterior,
                ordemAtualizada.Status,
                "Diagnostico iniciado.",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico iniciado com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(IniciarDiagnosticoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> FinalizarDiagnosticoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarDiagnosticoAsync), "Consultando ordem de servico para finalizar diagnostico");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(FinalizarDiagnosticoAsync), "Ordem de servico nao encontrada para finalizar diagnostico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarDiagnosticoAsync), "Validando composicao da OS e alterando status para AguardandoAprovacao");
            var statusAnterior = ordem.Status;
            ordem.FinalizarDiagnostico();
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.DiagnosticoFinalizado,
                statusAnterior,
                ordemAtualizada.Status,
                "Diagnostico finalizado e orcamento enviado para aprovacao.",
                cancellationToken);
            await RegistrarNotificacaoClienteAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.OrcamentoDisponivel,
                CanalNotificacaoCliente.WhatsApp,
                $"Orcamento disponivel para a ordem de servico {ordemAtualizada.Numero}. Endpoint de acompanhamento: {MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico finalizado com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(FinalizarDiagnosticoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> AprovarAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var ordemAtualizada = await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Consultando ordem de servico para aprovacao do orcamento");
                    var ordem = await _ordemRepository.ObterPorIdAsync(id, token);
                    if (ordem == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AprovarAsync), "Ordem de servico nao encontrada para aprovacao do orcamento");
                        throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
                    }

                    if (ordem.Status != StatusOrdemDeServico.AguardandoAprovacao &&
                        ordem.Status != StatusOrdemDeServico.AguardandoEstoque)
                    {
                        throw new InvalidOperationException($"A ordem de servico nao pode iniciar execucao no status atual: {ordem.Status}");
                    }

                    var statusAnterior = ordem.Status;
                    var faltasEstoque = await ObterFaltasDeEstoqueAsync(ordem, token);
                    if (faltasEstoque.Count > 0)
                    {
                        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Bloqueando aprovacao por falta de estoque e gerando pedidos de compra");
                        ordem.BloquearPorFaltaEstoque();
                        var ordemBloqueada = await _ordemRepository.AtualizarAsync(ordem, token);
                        await RegistrarHistoricoAsync(
                            ordemBloqueada,
                            TipoEventoOrdemServico.BloqueioPorFaltaEstoque,
                            statusAnterior,
                            ordemBloqueada.Status,
                            $"Execucao bloqueada por falta de estoque: {FormatarFaltasDeEstoque(faltasEstoque)}",
                            token);

                        foreach (var falta in faltasEstoque)
                        {
                            await CriarOuAtualizarPedidoCompraAsync(ordemBloqueada, falta.Peca, falta.QuantidadeFaltante, token);
                        }

                        return ordemBloqueada;
                    }

                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Baixando estoque das pecas e liberando ordem para execucao");
                    await BaixarEstoqueDaOrdemAsync(ordem, token);
                    ordem.LiberarExecucaoAposValidacaoEstoque();
                    var ordemExecutando = await _ordemRepository.AtualizarAsync(ordem, token);
                    await RegistrarHistoricoAsync(
                        ordemExecutando,
                        TipoEventoOrdemServico.OrcamentoAprovado,
                        statusAnterior,
                        ordemExecutando.Status,
                        "Orcamento aprovado pelo cliente e estoque validado com sucesso.",
                        token);
                    return ordemExecutando;
                },
                cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Orcamento aprovado com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AprovarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> FinalizarAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarAsync), "Consultando ordem de servico para finalizacao");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(FinalizarAsync), "Ordem de servico nao encontrada para finalizacao");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarAsync), "Finalizando servico e alterando status para Finalizada");
            var statusAnterior = ordem.Status;
            ordem.FinalizarServico();
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.ServicoFinalizado,
                statusAnterior,
                ordemAtualizada.Status,
                "Servico finalizado.",
                cancellationToken);
            await RegistrarNotificacaoClienteAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.ServicoFinalizado,
                CanalNotificacaoCliente.WhatsApp,
                $"Servico finalizado para a ordem de servico {ordemAtualizada.Numero}. Veiculo pronto para pagamento e retirada. Endpoint de acompanhamento: {MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico finalizado com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(FinalizarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> EntregarAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(EntregarAsync), "Consultando ordem de servico para entrega");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(EntregarAsync), "Ordem de servico nao encontrada para entrega");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(EntregarAsync), "Entregando veiculo e alterando status para Entregue");
            var statusAnterior = ordem.Status;
            ordem.Entregar();
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.VeiculoEntregue,
                statusAnterior,
                ordemAtualizada.Status,
                "Veiculo entregue ao cliente.",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Veiculo entregue com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(EntregarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> RegistrarPagamentoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(RegistrarPagamentoAsync), "Consultando ordem de servico para registro de pagamento");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(RegistrarPagamentoAsync), "Ordem de servico nao encontrada para registro de pagamento");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(RegistrarPagamentoAsync), "Registrando pagamento da ordem de servico");
            ordem.RegistrarPagamento();
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.PagamentoRegistrado,
                ordemAtualizada.Status,
                ordemAtualizada.Status,
                "Pagamento registrado para a ordem de servico.",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Pagamento registrado com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RegistrarPagamentoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> CancelarAsync(int id, CancelarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CancelarAsync), "Consultando ordem de servico para cancelamento");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CancelarAsync), "Ordem de servico nao encontrada para cancelamento");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CancelarAsync), "Cancelando ordem de servico com motivo informado");
            var statusAnterior = ordem.Status;
            ordem.Cancelar(dto.MotivoCancelamento);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.OrdemCancelada,
                statusAnterior,
                ordemAtualizada.Status,
                $"Ordem cancelada. Motivo: {ordemAtualizada.MotivoCancelamento}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico cancelada com sucesso. Numero: {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CancelarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> AdicionarServicoAsync(int id, AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarServicoAsync), "Consultando ordem de servico e servico para composicao do orcamento");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Ordem de servico nao encontrada para adicionar servico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            if (ordem.Servicos.Any(x => x.ServicoId == dto.ServicoId))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Servico ja adicionado a ordem de servico");
                throw new ServiceValidationException("O servico informado ja foi adicionado a ordem de servico.");
            }

            var servico = await _servicoRepository.ObterPorIdAsync(dto.ServicoId, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Servico nao encontrado para composicao do orcamento");
                throw new ServiceNotFoundException($"Servico com ID {dto.ServicoId} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarServicoAsync), "Adicionando servico a ordem");
            ordem.AdicionarServico(servico);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.ServicoAdicionado,
                ordemAtualizada.Status,
                ordemAtualizada.Status,
                $"Servico adicionado ao orcamento: {servico.Nome}.",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico adicionado com sucesso a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> AdicionarPecaAsync(int id, AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarPecaAsync), "Consultando ordem de servico e peca para composicao do orcamento");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarPecaAsync), "Ordem de servico nao encontrada para adicionar peca");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var peca = await _pecaRepository.ObterPorIdAsync(dto.PecaId, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarPecaAsync), "Peca nao encontrada para composicao do orcamento");
                throw new ServiceNotFoundException($"Peca com ID {dto.PecaId} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarPecaAsync), "Adicionando peca a ordem");
            ordem.AdicionarPeca(peca, dto.Quantidade);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.PecaAdicionada,
                ordemAtualizada.Status,
                ordemAtualizada.Status,
                $"Peca adicionada ao orcamento: {peca.Nome}. Quantidade: {dto.Quantidade}.",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Peca adicionada com sucesso a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarPecaAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> RemoverServicoAsync(int id, int servicoId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(RemoverServicoAsync), "Consultando ordem de servico para remover servico");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(RemoverServicoAsync), "Ordem de servico nao encontrada para remover servico");
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var servico = ordem.Servicos.FirstOrDefault(x => x.ServicoId == servicoId)?.Servico
                ?? await _servicoRepository.ObterPorIdAsync(servicoId, cancellationToken);

            ordem.RemoverServico(servicoId);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.ServicoAdicionado,
                ordemAtualizada.Status,
                ordemAtualizada.Status,
                $"Servico removido do orcamento: {servico?.Nome ?? servicoId.ToString()}.",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico removido com sucesso da ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RemoverServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> RemoverPecaAsync(int id, int pecaId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(RemoverPecaAsync), "Consultando ordem de servico para remover peca");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(RemoverPecaAsync), "Ordem de servico nao encontrada para remover peca");
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var peca = ordem.Pecas.FirstOrDefault(x => x.PecaId == pecaId)?.Peca
                ?? await _pecaRepository.ObterPorIdAsync(pecaId, cancellationToken);

            ordem.RemoverPeca(pecaId);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.PecaAdicionada,
                ordemAtualizada.Status,
                ordemAtualizada.Status,
                $"Peca removida do orcamento: {peca?.Nome ?? pecaId.ToString()}.",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Peca removida com sucesso da ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RemoverPecaAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private static string GerarNumeroTemporario()
    {
        return $"TMP-{Guid.NewGuid():N}";
    }

    private static string GerarNumeroOrdem(int id, DateTime dataAbertura)
    {
        return $"OS-{dataAbertura:yyyyMMdd}-{id}";
    }

    private static string MontarEndpointAcompanhamento(string codigoAcompanhamento)
    {
        return $"/api/v1/acompanhamento-os/{codigoAcompanhamento}";
    }

    private async Task<(string Codigo, string Token, string TokenHash)> GerarCredenciaisAcompanhamentoAsync(CancellationToken cancellationToken)
    {
        for (var tentativa = 0; tentativa < 5; tentativa++)
        {
            var codigo = $"AC-{StringHelper.GenerateSecureHexToken(8)}";
            var existente = await _ordemRepository.ObterPorCodigoAcompanhamentoAsync(codigo, cancellationToken);
            if (existente is not null)
            {
                continue;
            }

            var token = StringHelper.GenerateSecureHexToken(32);
            return (codigo, token, StringHelper.ToSha256Hash(token));
        }

        throw new InvalidOperationException("Nao foi possivel gerar credenciais de acompanhamento unicas.");
    }

    private async Task RegistrarHistoricoAsync(
        OrdemDeServico ordem,
        TipoEventoOrdemServico tipoEvento,
        StatusOrdemDeServico? statusAnterior,
        StatusOrdemDeServico? statusNovo,
        string descricao,
        CancellationToken cancellationToken)
    {
        var usuarioAtual = _usuarioAutenticadoService.ObterUsuarioAtual();

        await _historicoRepository.CriarAsync(
            new OrdemServicoHistorico
            {
                OrdemDeServicoId = ordem.Id,
                UsuarioId = usuarioAtual.UsuarioId,
                UsuarioNome = usuarioAtual.UsuarioNome,
                StatusAnterior = statusAnterior,
                StatusNovo = statusNovo,
                TipoEvento = tipoEvento,
                Descricao = descricao,
                DataEvento = DateTimeHelper.UTCBrazilNow()
            },
            cancellationToken);
    }

    private async Task RegistrarNotificacaoClienteAsync(
        int ordemDeServicoId,
        TipoNotificacaoCliente tipoNotificacao,
        CanalNotificacaoCliente canal,
        string mensagem,
        CancellationToken cancellationToken)
    {
        await _notificacaoClienteRepository.CriarAsync(
            new NotificacaoCliente
            {
                OrdemDeServicoId = ordemDeServicoId,
                DataNotificacao = DateTimeHelper.UTCBrazilNow(),
                Canal = canal,
                TipoNotificacao = tipoNotificacao,
                Mensagem = mensagem,
                Recebida = true
            },
            cancellationToken);
    }

    private async Task<List<FaltaEstoqueItem>> ObterFaltasDeEstoqueAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        var faltas = new List<FaltaEstoqueItem>();

        foreach (var item in ordem.Pecas)
        {
            var peca = await _pecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
            if (peca == null)
            {
                throw new KeyNotFoundException($"Peca com ID {item.PecaId} nao encontrada para validacao de estoque.");
            }

            if (peca.QuantidadeEstoque < item.Quantidade)
            {
                faltas.Add(new FaltaEstoqueItem(peca, item.Quantidade - peca.QuantidadeEstoque));
            }
        }

        return faltas;
    }

    private async Task BaixarEstoqueDaOrdemAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        if (!ordem.Pecas.Any())
        {
            return;
        }

        var movimentos = new List<string>();

        foreach (var item in ordem.Pecas)
        {
            var peca = await _pecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
            if (peca == null)
            {
                throw new KeyNotFoundException($"Peca com ID {item.PecaId} nao encontrada para baixa de estoque.");
            }

            var estoqueAnterior = peca.QuantidadeEstoque;
            peca.BaixarEstoque(item.Quantidade);
            await _pecaRepository.AtualizarAsync(peca, cancellationToken);
            await _movimentacaoEstoqueRepository.CriarAsync(
                new MovimentacaoEstoque
                {
                    PecaId = peca.Id,
                    OrdemDeServicoId = ordem.Id,
                    TipoMovimentacao = TipoMovimentacaoEstoque.BaixaParaOrdemDeServico,
                    Quantidade = item.Quantidade,
                    QuantidadeAnterior = estoqueAnterior,
                    QuantidadePosterior = peca.QuantidadeEstoque,
                    Descricao = $"Baixa de estoque para a ordem de servico {ordem.Numero}.",
                    DataMovimentacao = DateTimeHelper.UTCBrazilNow()
                },
                cancellationToken);
            movimentos.Add($"{peca.Nome} x{item.Quantidade}");
        }

        await RegistrarHistoricoAsync(
            ordem,
            TipoEventoOrdemServico.EstoqueBaixado,
            ordem.Status,
            ordem.Status,
            $"Baixa de estoque registrada para a ordem: {string.Join(", ", movimentos)}.",
            cancellationToken);
    }

    private async Task CriarOuAtualizarPedidoCompraAsync(OrdemDeServico ordem, Peca peca, int quantidadeFaltante, CancellationToken cancellationToken)
    {
        var pedidoExistente = await _pedidoCompraRepository.ObterPendentePorOrdemEPecaAsync(ordem.Id, peca.Id, cancellationToken);
        if (pedidoExistente == null)
        {
            var pedidoCriado = await _pedidoCompraRepository.CriarAsync(
                new PedidoCompra
                {
                    OrdemDeServicoId = ordem.Id,
                    PecaId = peca.Id,
                    QuantidadeSolicitada = quantidadeFaltante,
                    QuantidadeRecebida = 0,
                    Status = StatusPedidoCompra.Pendente,
                    DataSolicitacao = DateTimeHelper.UTCBrazilNow(),
                    Observacao = $"Pedido gerado automaticamente por falta de estoque para a ordem {ordem.Numero}."
                },
                cancellationToken);

            await RegistrarHistoricoAsync(
                ordem,
                TipoEventoOrdemServico.PedidoCompraGerado,
                ordem.Status,
                ordem.Status,
                $"Pedido de compra {pedidoCriado.Id} gerado para a peca {peca.Nome}. Quantidade solicitada: {quantidadeFaltante}.",
                cancellationToken);
            return;
        }

        if (pedidoExistente.QuantidadeSolicitada < quantidadeFaltante)
        {
            pedidoExistente.AtualizarQuantidadeSolicitada(quantidadeFaltante);
            await _pedidoCompraRepository.AtualizarAsync(pedidoExistente, cancellationToken);
        }

        await RegistrarHistoricoAsync(
            ordem,
            TipoEventoOrdemServico.PedidoCompraGerado,
            ordem.Status,
            ordem.Status,
            $"Pedido de compra pendente mantido para a peca {peca.Nome}. Quantidade solicitada: {pedidoExistente.QuantidadeSolicitada}.",
            cancellationToken);
    }

    private static string FormatarFaltasDeEstoque(IEnumerable<FaltaEstoqueItem> faltas)
    {
        return string.Join(", ", faltas.Select(x => $"{x.Peca.Nome} ({x.QuantidadeFaltante})"));
    }

    private static OrdemServicoHistoricoDto MapToDto(OrdemServicoHistorico historico)
    {
        return new OrdemServicoHistoricoDto
        {
            Id = historico.Id,
            OrdemDeServicoId = historico.OrdemDeServicoId,
            UsuarioId = historico.UsuarioId,
            UsuarioNome = historico.UsuarioNome,
            StatusAnterior = historico.StatusAnterior?.ToString(),
            StatusNovo = historico.StatusNovo?.ToString(),
            TipoEvento = historico.TipoEvento.ToString(),
            Descricao = historico.Descricao,
            DataEvento = historico.DataEvento
        };
    }

    private static NotificacaoClienteDto MapToDto(NotificacaoCliente notificacao)
    {
        return new NotificacaoClienteDto
        {
            Id = notificacao.Id,
            OrdemDeServicoId = notificacao.OrdemDeServicoId,
            Canal = notificacao.Canal.ToString(),
            TipoNotificacao = notificacao.TipoNotificacao.ToString(),
            Mensagem = notificacao.Mensagem,
            Recebida = notificacao.Recebida,
            DataNotificacao = notificacao.DataNotificacao
        };
    }

    private static OrdemDeServicoDto MapToDto(OrdemDeServico ordem)
    {
        return new OrdemDeServicoDto
        {
            Id = ordem.Id,
            Numero = ordem.Numero,
            CodigoAcompanhamento = ordem.CodigoAcompanhamento,
            UrlAcompanhamento = MontarEndpointAcompanhamento(ordem.CodigoAcompanhamento),
            TokenAcompanhamento = null,
            ClienteId = ordem.ClienteId,
            VeiculoId = ordem.VeiculoId,
            DescricaoSolicitacao = ordem.DescricaoSolicitacao,
            ObservacoesRecepcao = ordem.ObservacoesRecepcao,
            MotivoCancelamento = ordem.MotivoCancelamento,
            OrcamentoEnviadoEm = ordem.OrcamentoEnviadoEm,
            DataFinalizacao = ordem.DataFinalizacao,
            DataPagamento = ordem.DataPagamento,
            Status = ordem.Status.ToString(),
            DataAbertura = ordem.DataAbertura,
            DataConclusao = ordem.DataConclusao,
            ValorTotal = ordem.ValorTotal,
            Servicos = ordem.Servicos.Select(s => new OrdemDeServicoServicoDto
            {
                ServicoId = s.ServicoId,
                Preco = s.Preco,
                TempoEstimado = s.TempoEstimado
            }).ToList(),
            Pecas = ordem.Pecas.Select(p => new OrdemDeServicoPecaDto
            {
                PecaId = p.PecaId,
                Quantidade = p.Quantidade,
                Preco = p.Preco
            }).ToList()
        };
    }

    private static MovimentacaoEstoqueDto MapToDto(MovimentacaoEstoque movimentacao)
    {
        return new MovimentacaoEstoqueDto
        {
            Id = movimentacao.Id,
            PecaId = movimentacao.PecaId,
            OrdemDeServicoId = movimentacao.OrdemDeServicoId,
            PedidoCompraId = movimentacao.PedidoCompraId,
            NomePeca = movimentacao.Peca?.Nome ?? string.Empty,
            TipoMovimentacao = movimentacao.TipoMovimentacao.ToString(),
            Quantidade = movimentacao.Quantidade,
            QuantidadeAnterior = movimentacao.QuantidadeAnterior,
            QuantidadePosterior = movimentacao.QuantidadePosterior,
            Descricao = movimentacao.Descricao,
            DataMovimentacao = movimentacao.DataMovimentacao
        };
    }

    private static MonitoramentoOrdemDeServicoDto MapToMonitoramentoDto(OrdemDeServico ordem, DateTime agora)
    {
        var dataReferencia = ordem.DataFinalizacao ?? agora;
        var tempoDecorrido = dataReferencia - ordem.DataAbertura;
        var tempoFinalizacao = ordem.DataFinalizacao.HasValue
            ? ordem.DataFinalizacao.Value - ordem.DataAbertura
            : (TimeSpan?)null;

        return new MonitoramentoOrdemDeServicoDto
        {
            Id = ordem.Id,
            Numero = ordem.Numero,
            Status = ordem.Status.ToString(),
            DataAbertura = ordem.DataAbertura,
            DataFinalizacao = ordem.DataFinalizacao,
            EstaFinalizada = ordem.DataFinalizacao.HasValue,
            TempoDecorridoMinutos = Math.Max(0, (int)Math.Round(tempoDecorrido.TotalMinutes)),
            TempoDecorridoHoras = Math.Max(0, Math.Round(tempoDecorrido.TotalHours, 2)),
            TempoFinalizacaoMinutos = tempoFinalizacao.HasValue
                ? Math.Max(0, (int)Math.Round(tempoFinalizacao.Value.TotalMinutes))
                : null,
            TempoFinalizacaoHoras = tempoFinalizacao.HasValue
                ? Math.Max(0, Math.Round(tempoFinalizacao.Value.TotalHours, 2))
                : null
        };
    }

    private sealed record FaltaEstoqueItem(Peca Peca, int QuantidadeFaltante);
}




