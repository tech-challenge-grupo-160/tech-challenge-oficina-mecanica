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
}

public class OrdemDeServicoApplicationService : IOrdemDeServicoApplicationService
{
    private const string LoggerName = nameof(OrdemDeServicoApplicationService);
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly IUsuarioAutenticadoService _usuarioAutenticadoService;
    private readonly ILogger _logger;

    public OrdemDeServicoApplicationService(
        IOrdemDeServicoRepository ordemRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IPecaRepository pecaRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        ILoggerFactory loggerFactory)
    {
        _ordemRepository = ordemRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _pecaRepository = pecaRepository;
        _historicoRepository = historicoRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
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

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Persistindo ordem de servico em status Recebida");
            var ordem = new OrdemDeServico
            {
                Numero = GerarNumeroTemporario(),
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
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico aberta com sucesso. Numero: {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
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
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Consultando ordem de servico para aprovacao do orcamento");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AprovarAsync), "Ordem de servico nao encontrada para aprovacao do orcamento");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Aprovando orcamento e alterando status para EmExecucao");
            var statusAnterior = ordem.Status;
            ordem.AprovarOrcamento();
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await RegistrarHistoricoAsync(
                ordemAtualizada,
                TipoEventoOrdemServico.OrcamentoAprovado,
                statusAnterior,
                ordemAtualizada.Status,
                "Orcamento aprovado pelo cliente.",
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

    private static string GerarNumeroTemporario()
    {
        return $"TMP-{Guid.NewGuid():N}";
    }

    private static string GerarNumeroOrdem(int id, DateTime dataAbertura)
    {
        return $"OS-{dataAbertura:yyyyMMdd}-{id}";
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

    private static OrdemDeServicoDto MapToDto(OrdemDeServico ordem)
    {
        return new OrdemDeServicoDto
        {
            Id = ordem.Id,
            Numero = ordem.Numero,
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
}




