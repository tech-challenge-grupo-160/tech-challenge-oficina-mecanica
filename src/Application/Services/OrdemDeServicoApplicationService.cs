using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IOrdemDeServicoApplicationService
{
    Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(int id, CancellationToken cancellationToken);
    Task<PagedResultDto<OrdemDeServicoDto>> ListarOrdensDeServicoAsync(
        int page,
        int pageSize,
        int? clienteId,
        string? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorStatusAsync(string status, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> IniciarDiagnosticoAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> FinalizarDiagnosticoAsync(int id, CancellationToken cancellationToken);
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
    private readonly ILogger _logger;

    public OrdemDeServicoApplicationService(
        IOrdemDeServicoRepository ordemRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IPecaRepository pecaRepository,
        ILoggerFactory loggerFactory)
    {
        _ordemRepository = ordemRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _pecaRepository = pecaRepository;
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
                throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} nao encontrado.");
            }

            var veiculo = await _veiculoRepository.ObterPorIdAsync(dto.VeiculoId, cancellationToken);
            if (veiculo == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao encontrado para abertura da OS");
                throw new KeyNotFoundException($"Veiculo com ID {dto.VeiculoId} nao encontrado.");
            }

            if (veiculo.ClienteId != dto.ClienteId)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao pertence ao cliente informado");
                throw new InvalidOperationException("O veiculo informado nao pertence ao cliente informado.");
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
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico aberta com sucesso. Numero: {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarOrdemDeServicoAsync), ex.Message);
            throw;
        }
    }

    public async Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return MapToDto(ordem);
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
                throw new InvalidOperationException($"Status invalido: {status}");
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

    public async Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {clienteId} nao encontrado.");
        }

        var ordens = await _ordemRepository.ObterPorClienteAsync(clienteId, cancellationToken);
        return ordens.Select(MapToDto);
    }

    public async Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorStatusAsync(string status, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<StatusOrdemDeServico>(status, true, out var statusEnum))
        {
            throw new InvalidOperationException($"Status invalido: {status}");
        }

        var ordens = await _ordemRepository.ObterPorStatusAsync(statusEnum, cancellationToken);
        return ordens.Select(MapToDto);
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
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            if (!Enum.TryParse<StatusOrdemDeServico>(dto.NovoStatus, out var novoStatus))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AtualizarStatusAsync), "Status informado e invalido");
                throw new InvalidOperationException($"Status invalido: {dto.NovoStatus}");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarStatusAsync), $"Alterando status da ordem para {novoStatus}");
            ordem.AlterarStatus(novoStatus);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Status da ordem atualizado com sucesso para {ordemAtualizada.Status}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AtualizarStatusAsync), ex.Message);
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
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(IniciarDiagnosticoAsync), "Alterando status da ordem para EmDiagnostico");
            ordem.AlterarStatus(StatusOrdemDeServico.EmDiagnostico);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico iniciado com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(IniciarDiagnosticoAsync), ex.Message);
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
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarDiagnosticoAsync), "Alterando status da ordem para AguardandoAprovacao");
            ordem.AlterarStatus(StatusOrdemDeServico.AguardandoAprovacao);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico finalizado com sucesso para a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(FinalizarDiagnosticoAsync), ex.Message);
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
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CancelarAsync), "Cancelando ordem de servico com motivo informado");
            ordem.Cancelar(dto.MotivoCancelamento);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico cancelada com sucesso. Numero: {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CancelarAsync), ex.Message);
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
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var servico = await _servicoRepository.ObterPorIdAsync(dto.ServicoId, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Servico nao encontrado para composicao do orcamento");
                throw new KeyNotFoundException($"Servico com ID {dto.ServicoId} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarServicoAsync), "Adicionando servico a ordem");
            ordem.AdicionarServico(servico);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico adicionado com sucesso a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarServicoAsync), ex.Message);
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
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var peca = await _pecaRepository.ObterPorIdAsync(dto.PecaId, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarPecaAsync), "Peca nao encontrada para composicao do orcamento");
                throw new KeyNotFoundException($"Peca com ID {dto.PecaId} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarPecaAsync), "Adicionando peca a ordem");
            ordem.AdicionarPeca(peca, dto.Quantidade);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Peca adicionada com sucesso a ordem {ordemAtualizada.Numero}");
            return MapToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarPecaAsync), ex.Message);
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
}
