using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IServicoApplicationService
{
    Task<ServicoDto> CriarServicoAsync(CriarServicoDto dto, CancellationToken cancellationToken);
    Task<ServicoDto> ObterServicoAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<ServicoDto>> ListarServicosAsync(CancellationToken cancellationToken);
    Task<ServicoDto> AtualizarServicoAsync(int id, AtualizarServicoDto dto, CancellationToken cancellationToken);
    Task DeletarServicoAsync(int id, CancellationToken cancellationToken);
}

public class ServicoApplicationService : IServicoApplicationService
{
    private const string LoggerName = nameof(ServicoApplicationService);
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public ServicoApplicationService(IServicoRepository servicoRepository, ILoggerFactory loggerFactory)
    {
        _servicoRepository = servicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ServicoDto> CriarServicoAsync(CriarServicoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarServicoAsync), "Persistindo novo servico");
            var servico = new Servico
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Preco = dto.Preco,
                TempoEstimado = dto.TempoEstimado
            };

            var servicoCriado = await _servicoRepository.CriarAsync(servico, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico criado com sucesso.");
            return MapToDto(servicoCriado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<ServicoDto> ObterServicoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ObterServicoAsync), "Consultando servico por identificador");
            var servico = await _servicoRepository.ObterPorIdAsync(id, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterServicoAsync), "Servico nao encontrado para o identificador informado");
                throw new ServiceNotFoundException($"Servico com ID {id} nao encontrado.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico obtido com sucesso.");
            return MapToDto(servico);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ObterServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<ServicoDto>> ListarServicosAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ListarServicosAsync), "Consultando todos os servicos");
            var servicos = await _servicoRepository.ObterTodosAsync(cancellationToken);
            var resultado = servicos.Select(MapToDto).ToArray();
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta de servicos concluida. Total de registros: {resultado.Length}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ListarServicosAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task<ServicoDto> AtualizarServicoAsync(int id, AtualizarServicoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarServicoAsync), "Consultando servico para atualizacao");
            var servico = await _servicoRepository.ObterPorIdAsync(id, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AtualizarServicoAsync), "Servico nao encontrado para atualizacao");
                throw new ServiceNotFoundException($"Servico com ID {id} nao encontrado.");
            }

            servico.Nome = dto.Nome;
            servico.Descricao = dto.Descricao;
            servico.Preco = dto.Preco;
            servico.TempoEstimado = dto.TempoEstimado;

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarServicoAsync), "Persistindo atualizacao do servico");
            var servicoAtualizado = await _servicoRepository.AtualizarAsync(servico, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico atualizado com sucesso.");
            return MapToDto(servicoAtualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AtualizarServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    public async Task DeletarServicoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarServicoAsync), "Consultando servico para exclusao");
            var servico = await _servicoRepository.ObterPorIdAsync(id, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarServicoAsync), "Servico nao encontrado para exclusao");
                throw new ServiceNotFoundException($"Servico com ID {id} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarServicoAsync), "Validando se existem ordens de servico ativas vinculadas ao servico");
            if (await _servicoRepository.ExisteEmOrdemDeServicoAtivaAsync(id, cancellationToken))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarServicoAsync), "Servico possui ordens de servico ativas vinculadas");
                throw new ServiceValidationException("Nao e possivel excluir o servico pois existem ordens de servico ativas vinculadas.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarServicoAsync), "Excluindo servico");
            await _servicoRepository.DeletarAsync(id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico excluido com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(DeletarServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private static ServicoDto MapToDto(Servico servico)
    {
        return new ServicoDto
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Preco = servico.Preco,
            TempoEstimado = servico.TempoEstimado
        };
    }
}





