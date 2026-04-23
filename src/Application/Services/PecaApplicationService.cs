using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IPecaApplicationService
{
    Task<PecaDto> CriarPecaAsync(CriarPecaDto dto, CancellationToken cancellationToken);
    Task<PecaDto> ObterPecaAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<PecaDto>> ListarPecasAsync(CancellationToken cancellationToken);
    Task<PecaDto> AtualizarPecaAsync(int id, AtualizarPecaDto dto, CancellationToken cancellationToken);
    Task DeletarPecaAsync(int id, CancellationToken cancellationToken);
}

public class PecaApplicationService : IPecaApplicationService
{
    private const string LoggerName = nameof(PecaApplicationService);
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public PecaApplicationService(IPecaRepository pecaRepository, ILoggerFactory loggerFactory)
    {
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PecaDto> CriarPecaAsync(CriarPecaDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarPecaAsync), "Persistindo nova peca");
            var peca = new Peca
            {
                Nome = dto.Nome,
                Preco = dto.Preco,
                QuantidadeEstoque = dto.QuantidadeEstoque
            };

            var pecaCriada = await _pecaRepository.CriarAsync(peca, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca criada com sucesso.");
            return MapToDto(pecaCriada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarPecaAsync), ex.Message);
            throw;
        }
    }

    public async Task<PecaDto> ObterPecaAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ObterPecaAsync), "Consultando peca por identificador");
            var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ObterPecaAsync), "Peca nao encontrada para o identificador informado");
                throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca obtida com sucesso.");
            return MapToDto(peca);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ObterPecaAsync), ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<PecaDto>> ListarPecasAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ListarPecasAsync), "Consultando todas as pecas");
            var pecas = await _pecaRepository.ObterTodosAsync(cancellationToken);
            var resultado = pecas.Select(MapToDto).ToArray();
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta de pecas concluida. Total de registros: {resultado.Length}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ListarPecasAsync), ex.Message);
            throw;
        }
    }

    public async Task<PecaDto> AtualizarPecaAsync(int id, AtualizarPecaDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarPecaAsync), "Consultando peca para atualizacao");
            var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AtualizarPecaAsync), "Peca nao encontrada para atualizacao");
                throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
            }

            peca.Nome = dto.Nome;
            peca.Preco = dto.Preco;
            peca.QuantidadeEstoque = dto.QuantidadeEstoque;

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AtualizarPecaAsync), "Persistindo atualizacao da peca");
            var pecaAtualizada = await _pecaRepository.AtualizarAsync(peca, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca atualizada com sucesso.");
            return MapToDto(pecaAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AtualizarPecaAsync), ex.Message);
            throw;
        }
    }

    public async Task DeletarPecaAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarPecaAsync), "Consultando peca para exclusao");
            var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(DeletarPecaAsync), "Peca nao encontrada para exclusao");
                throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(DeletarPecaAsync), "Excluindo peca");
            await _pecaRepository.DeletarAsync(id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca excluida com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(DeletarPecaAsync), ex.Message);
            throw;
        }
    }

    private static PecaDto MapToDto(Peca peca)
    {
        return new PecaDto
        {
            Id = peca.Id,
            Nome = peca.Nome,
            Preco = peca.Preco,
            QuantidadeEstoque = peca.QuantidadeEstoque
        };
    }
}
