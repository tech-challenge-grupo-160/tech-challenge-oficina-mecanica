using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class OrdemDeServicoHandlerDependencies
{
    public OrdemDeServicoHandlerDependencies(
        IOrdemDeServicoRepository ordemRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IPecaRepository pecaRepository,
        IPedidoCompraRepository pedidoCompraRepository,
        IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        INotificacaoClienteRepository notificacaoClienteRepository,
        ITransactionManager transactionManager,
        IClock clock,
        ILoggerFactory loggerFactory,
        OrdemDeServicoAcompanhamentoService acompanhamentoService,
        OrdemDeServicoHistoricoService historicoService,
        OrdemDeServicoNotificacaoService notificacaoService,
        OrdemDeServicoEstoqueService estoqueService)
    {
        OrdemRepository = ordemRepository;
        ClienteRepository = clienteRepository;
        VeiculoRepository = veiculoRepository;
        ServicoRepository = servicoRepository;
        PecaRepository = pecaRepository;
        PedidoCompraRepository = pedidoCompraRepository;
        MovimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
        HistoricoRepository = historicoRepository;
        NotificacaoClienteRepository = notificacaoClienteRepository;
        TransactionManager = transactionManager;
        Clock = clock;
        LoggerFactory = loggerFactory;
        AcompanhamentoService = acompanhamentoService;
        HistoricoService = historicoService;
        NotificacaoService = notificacaoService;
        EstoqueService = estoqueService;
    }

    public IOrdemDeServicoRepository OrdemRepository { get; }
    public IClienteRepository ClienteRepository { get; }
    public IVeiculoRepository VeiculoRepository { get; }
    public IServicoRepository ServicoRepository { get; }
    public IPecaRepository PecaRepository { get; }
    public IPedidoCompraRepository PedidoCompraRepository { get; }
    public IMovimentacaoEstoqueRepository MovimentacaoEstoqueRepository { get; }
    public IOrdemServicoHistoricoRepository HistoricoRepository { get; }
    public INotificacaoClienteRepository NotificacaoClienteRepository { get; }
    public ITransactionManager TransactionManager { get; }
    public IClock Clock { get; }
    public ILoggerFactory LoggerFactory { get; }
    public OrdemDeServicoAcompanhamentoService AcompanhamentoService { get; }
    public OrdemDeServicoHistoricoService HistoricoService { get; }
    public OrdemDeServicoNotificacaoService NotificacaoService { get; }
    public OrdemDeServicoEstoqueService EstoqueService { get; }
}

public abstract class OrdemDeServicoHandlerBase
{
    private const string LoggerName = nameof(OrdemDeServicoHandlerBase);
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly INotificacaoClienteRepository _notificacaoClienteRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly OrdemDeServicoAcompanhamentoService _acompanhamentoService;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly OrdemDeServicoNotificacaoService _notificacaoService;
    private readonly OrdemDeServicoEstoqueService _estoqueService;

    protected OrdemDeServicoHandlerBase(OrdemDeServicoHandlerDependencies dependencies)
    {
        _ordemRepository = dependencies.OrdemRepository;
        _clienteRepository = dependencies.ClienteRepository;
        _veiculoRepository = dependencies.VeiculoRepository;
        _servicoRepository = dependencies.ServicoRepository;
        _pecaRepository = dependencies.PecaRepository;
        _movimentacaoEstoqueRepository = dependencies.MovimentacaoEstoqueRepository;
        _historicoRepository = dependencies.HistoricoRepository;
        _notificacaoClienteRepository = dependencies.NotificacaoClienteRepository;
        _transactionManager = dependencies.TransactionManager;
        _clock = dependencies.Clock;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
        _acompanhamentoService = dependencies.AcompanhamentoService;
        _historicoService = dependencies.HistoricoService;
        _notificacaoService = dependencies.NotificacaoService;
        _estoqueService = dependencies.EstoqueService;
    }

    protected async Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken)
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
                await _acompanhamentoService.GerarCredenciaisAsync(cancellationToken);

            var ordem = OrdemDeServico.Criar(
                OrdemDeServicoAcompanhamentoService.GerarNumeroTemporario(),
                codigoAcompanhamento,
                tokenAcompanhamentoHash,
                dto.ClienteId,
                dto.VeiculoId,
                dto.DescricaoSolicitacao,
                dto.ObservacoesRecepcao,
                _clock.Now);

            var ordemCriada = await _ordemRepository.CriarAsync(ordem, cancellationToken);
            ordemCriada.DefinirNumero(OrdemDeServicoAcompanhamentoService.GerarNumeroOrdem(ordemCriada.Id, ordemCriada.DataAbertura));

            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordemCriada, cancellationToken);
            var eventoCriacao = ordemAtualizada.CriarEventoOrdemCriada();
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoCriacao.TipoEvento,
                eventoCriacao.StatusAnterior,
                eventoCriacao.StatusNovo,
                eventoCriacao.Descricao,
                cancellationToken);
            await _notificacaoService.RegistrarAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.LinkAcompanhamentoEnviado,
                CanalNotificacaoCliente.Email,
                $"Link de acompanhamento da ordem {ordemAtualizada.Numero} enviado para o e-mail {cliente.Email}. Endpoint: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico aberta com sucesso. Numero: {ordemAtualizada.Numero}");
            var resposta = OrdemDeServicoMapper.ToDto(ordemAtualizada);
            resposta.TokenAcompanhamento = tokenAcompanhamento;
            return resposta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarOrdemDeServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return OrdemDeServicoMapper.ToDto(ordem);
    }

    protected async Task<IEnumerable<OrdemServicoHistoricoDto>> ObterHistoricoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var historicos = await _historicoRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return historicos.Select(OrdemDeServicoMapper.ToDto);
    }

    protected async Task<IEnumerable<NotificacaoClienteDto>> ObterNotificacoesAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var notificacoes = await _notificacaoClienteRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return notificacoes.Select(OrdemDeServicoMapper.ToDto);
    }

    protected async Task<IEnumerable<MovimentacoesEstoquePorPecaDto>> ObterMovimentacoesEstoqueAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var movimentacoes = (await _movimentacaoEstoqueRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken))
            .Select(OrdemDeServicoMapper.ToDto)
            .GroupBy(x => x.PecaId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var grupos = new List<MovimentacoesEstoquePorPecaDto>();

        foreach (var item in ordem.Pecas.OrderBy(x => x.PecaId))
        {
            var peca = item.Peca ?? await _pecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
            var movimentacoesDaPeca = movimentacoes.TryGetValue(item.PecaId, out var valores)
                ? valores
                : new List<MovimentacaoEstoqueDto>();

            grupos.Add(new MovimentacoesEstoquePorPecaDto
            {
                PecaId = item.PecaId,
                NomePeca = peca?.Nome ?? movimentacoesDaPeca.FirstOrDefault()?.NomePeca ?? string.Empty,
                MarcaPeca = peca?.Marca ?? string.Empty,
                ModeloPeca = peca?.Modelo ?? string.Empty,
                QuantidadeNaOrdem = item.Quantidade,
                TotalMovimentacoes = movimentacoesDaPeca.Count,
                Movimentacoes = movimentacoesDaPeca
            });
        }

        return grupos;
    }

    protected async Task<MonitoramentoOrdemDeServicoDto> ObterMonitoramentoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return OrdemDeServicoMapper.ToMonitoramentoDto(ordem, _clock.Now);
    }

    protected async Task<EstimativaTempoOrdemDeServicoDto> ObterEstimativaTempoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return OrdemDeServicoMapper.ToEstimativaTempoDto(ordem);
    }

    protected async Task<ResumoMonitoramentoOrdensDeServicoDto> ObterResumoMonitoramentoAsync(int page, int pageSize, CancellationToken cancellationToken)
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

    protected async Task<PagedResultDto<OrdemDeServicoDto>> ListarOrdensDeServicoAsync(
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
            Items = ordens.Select(OrdemDeServicoMapper.ToDto).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    protected async Task<OrdemDeServicoDto> AtualizarStatusAsync(int id, AtualizarStatusOrdemDeServicoDto dto, CancellationToken cancellationToken)
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

            if (novoStatus == StatusOrdemDeServico.EmExecucao)
            {
                throw new InvalidOperationException("Nao e permitido alterar uma ordem de servico diretamente para EmExecucao. Use as rotas de aprovacao ou liberacao de execucao para garantir a validacao e baixa de estoque.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarStatusAsync), $"Alterando status da ordem para {novoStatus}");
            ordem.AlterarStatus(
                novoStatus,
                novoStatus == StatusOrdemDeServico.Entregue ? _clock.Now : null);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Status da ordem atualizado com sucesso para {ordemAtualizada.Status}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AtualizarStatusAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> IniciarDiagnosticoAsync(int id, CancellationToken cancellationToken)
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
            var eventoDiagnosticoIniciado = ordem.IniciarDiagnostico();
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoDiagnosticoIniciado.TipoEvento,
                eventoDiagnosticoIniciado.StatusAnterior,
                eventoDiagnosticoIniciado.StatusNovo,
                eventoDiagnosticoIniciado.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico iniciado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(IniciarDiagnosticoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> FinalizarDiagnosticoAsync(int id, CancellationToken cancellationToken)
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
            var eventoDiagnosticoFinalizado = ordem.FinalizarDiagnosticoComEvento(_clock.Now);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoDiagnosticoFinalizado.TipoEvento,
                eventoDiagnosticoFinalizado.StatusAnterior,
                eventoDiagnosticoFinalizado.StatusNovo,
                eventoDiagnosticoFinalizado.Descricao,
                cancellationToken);
            await _notificacaoService.RegistrarAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.OrcamentoDisponivel,
                CanalNotificacaoCliente.WhatsApp,
                $"Orcamento disponivel para a ordem de servico {ordemAtualizada.Numero}. Endpoint de acompanhamento: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico finalizado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(FinalizarDiagnosticoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> AprovarAsync(int id, CancellationToken cancellationToken)
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

                    if (ordem.Status == StatusOrdemDeServico.AguardandoEstoque)
                    {
                        throw new InvalidOperationException("A ordem de servico esta aguardando estoque. Use a rota de liberacao de execucao apos reposicao do estoque.");
                    }

                    if (ordem.Status != StatusOrdemDeServico.AguardandoAprovacao)
                    {
                        throw new InvalidOperationException($"A ordem de servico nao pode ser aprovada no status atual: {ordem.Status}");
                    }

                    var faltasEstoque = await _estoqueService.ObterFaltasAsync(ordem, token);
                    if (faltasEstoque.Count > 0)
                    {
                        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Bloqueando aprovacao por falta de estoque e gerando pedidos de compra");
                        var eventoBloqueio = ordem.BloquearPorFaltaEstoqueComEvento(OrdemDeServicoEstoqueService.FormatarFaltas(faltasEstoque));
                        var ordemBloqueada = await _ordemRepository.AtualizarAsync(ordem, token);
                        await _historicoService.RegistrarAsync(
                            ordemBloqueada,
                            eventoBloqueio.TipoEvento,
                            eventoBloqueio.StatusAnterior,
                            eventoBloqueio.StatusNovo,
                            eventoBloqueio.Descricao,
                            token);

                        foreach (var falta in faltasEstoque)
                        {
                            await _estoqueService.CriarOuAtualizarPedidoCompraAsync(ordemBloqueada, falta.Peca, falta.QuantidadeFaltante, token);
                        }

                        return ordemBloqueada;
                    }

                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Baixando estoque das pecas e liberando ordem para execucao");
                    await _estoqueService.BaixarEstoqueDaOrdemAsync(ordem, token);
                    var eventoAprovacao = ordem.LiberarExecucaoComEvento();
                    var ordemExecutando = await _ordemRepository.AtualizarAsync(ordem, token);
                    await _historicoService.RegistrarAsync(
                        ordemExecutando,
                        eventoAprovacao.TipoEvento,
                        eventoAprovacao.StatusAnterior,
                        eventoAprovacao.StatusNovo,
                        eventoAprovacao.Descricao,
                        token);
                    return ordemExecutando;
                },
                cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Orcamento aprovado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AprovarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> LiberarExecucaoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var ordemAtualizada = await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(LiberarExecucaoAsync), "Consultando ordem de servico aguardando estoque para liberacao de execucao");
                    var ordem = await _ordemRepository.ObterPorIdAsync(id, token);
                    if (ordem == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(LiberarExecucaoAsync), "Ordem de servico nao encontrada para liberacao de execucao");
                        throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
                    }

                    if (ordem.Status != StatusOrdemDeServico.AguardandoEstoque)
                    {
                        throw new InvalidOperationException($"A ordem de servico so pode ser liberada para execucao quando estiver aguardando estoque. Status atual: {ordem.Status}");
                    }

                    var faltasEstoque = await _estoqueService.ObterFaltasAsync(ordem, token);
                    if (faltasEstoque.Count > 0)
                    {
                        throw new InvalidOperationException($"Estoque indisponivel para liberar execucao da ordem de servico: {OrdemDeServicoEstoqueService.FormatarFaltas(faltasEstoque)}.");
                    }

                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(LiberarExecucaoAsync), "Baixando estoque das pecas e liberando ordem para execucao");
                    await _estoqueService.BaixarEstoqueDaOrdemAsync(ordem, token);
                    var eventoLiberacao = ordem.LiberarExecucaoComEvento();
                    var ordemExecutando = await _ordemRepository.AtualizarAsync(ordem, token);
                    await _historicoService.RegistrarAsync(
                        ordemExecutando,
                        eventoLiberacao.TipoEvento,
                        eventoLiberacao.StatusAnterior,
                        eventoLiberacao.StatusNovo,
                        eventoLiberacao.Descricao,
                        token);
                    return ordemExecutando;
                },
                cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Execucao liberada com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(LiberarExecucaoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> FinalizarAsync(int id, CancellationToken cancellationToken)
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
            var eventoFinalizacao = ordem.FinalizarServicoComEvento(_clock.Now);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoFinalizacao.TipoEvento,
                eventoFinalizacao.StatusAnterior,
                eventoFinalizacao.StatusNovo,
                eventoFinalizacao.Descricao,
                cancellationToken);
            await _notificacaoService.RegistrarAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.ServicoFinalizado,
                CanalNotificacaoCliente.WhatsApp,
                $"Servico finalizado para a ordem de servico {ordemAtualizada.Numero}. Veiculo pronto para pagamento e retirada. Endpoint de acompanhamento: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico finalizado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(FinalizarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> EntregarAsync(int id, CancellationToken cancellationToken)
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
            var eventoEntrega = ordem.EntregarComEvento(_clock.Now);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoEntrega.TipoEvento,
                eventoEntrega.StatusAnterior,
                eventoEntrega.StatusNovo,
                eventoEntrega.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Veiculo entregue com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(EntregarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> RegistrarPagamentoAsync(int id, CancellationToken cancellationToken)
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
            var eventoPagamento = ordem.RegistrarPagamentoComEvento(_clock.Now);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoPagamento.TipoEvento,
                eventoPagamento.StatusAnterior,
                eventoPagamento.StatusNovo,
                eventoPagamento.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Pagamento registrado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RegistrarPagamentoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> CancelarAsync(int id, CancelarOrdemDeServicoDto dto, CancellationToken cancellationToken)
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
            var eventoCancelamento = ordem.CancelarComEvento(dto.MotivoCancelamento);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoCancelamento.TipoEvento,
                eventoCancelamento.StatusAnterior,
                eventoCancelamento.StatusNovo,
                eventoCancelamento.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico cancelada com sucesso. Numero: {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CancelarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> AdicionarServicoAsync(int id, AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken)
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
            var eventoServicoAdicionado = ordem.AdicionarServicoComEvento(servico);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoServicoAdicionado.TipoEvento,
                eventoServicoAdicionado.StatusAnterior,
                eventoServicoAdicionado.StatusNovo,
                eventoServicoAdicionado.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico adicionado com sucesso a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> AdicionarPecaAsync(int id, AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken)
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
            var eventoPecaAdicionada = ordem.AdicionarPecaComEvento(peca, dto.Quantidade);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoPecaAdicionada.TipoEvento,
                eventoPecaAdicionada.StatusAnterior,
                eventoPecaAdicionada.StatusNovo,
                eventoPecaAdicionada.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Peca adicionada com sucesso a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarPecaAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> RemoverServicoAsync(int id, int servicoId, CancellationToken cancellationToken)
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

            var eventoServicoRemovido = ordem.RemoverServicoComEvento(servicoId, servico?.Nome ?? servicoId.ToString());
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoServicoRemovido.TipoEvento,
                eventoServicoRemovido.StatusAnterior,
                eventoServicoRemovido.StatusNovo,
                eventoServicoRemovido.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico removido com sucesso da ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RemoverServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    protected async Task<OrdemDeServicoDto> RemoverPecaAsync(int id, int pecaId, CancellationToken cancellationToken)
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

            var eventoPecaRemovida = ordem.RemoverPecaComEvento(pecaId, peca?.Nome ?? pecaId.ToString());
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoPecaRemovida.TipoEvento,
                eventoPecaRemovida.StatusAnterior,
                eventoPecaRemovida.StatusNovo,
                eventoPecaRemovida.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Peca removida com sucesso da ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RemoverPecaAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

}





