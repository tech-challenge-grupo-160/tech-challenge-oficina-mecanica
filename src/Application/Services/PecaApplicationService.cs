using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;

namespace oficina_mecanica.Application.Services;

public interface IPecaApplicationService
{
    Task<PecaDto> CriarPecaAsync(CriarPecaDto dto, CancellationToken cancellationToken);
    Task<PecaDto> ObterPecaAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<PecaDto>> ListarPecasAsync(CancellationToken cancellationToken);
    Task<PecaDto> AtualizarPecaAsync(Guid id, AtualizarPecaDto dto, CancellationToken cancellationToken);
    Task DeletarPecaAsync(Guid id, CancellationToken cancellationToken);
}

public class PecaApplicationService : IPecaApplicationService
{
    private readonly IPecaRepository _pecaRepository;

    public PecaApplicationService(IPecaRepository pecaRepository)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<PecaDto> CriarPecaAsync(CriarPecaDto dto, CancellationToken cancellationToken)
    {
        var peca = new Peca
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Preco = dto.Preco,
            QuantidadeEstoque = dto.QuantidadeEstoque
        };

        var pecaCriada = await _pecaRepository.CriarAsync(peca, cancellationToken);
        return MapToDto(pecaCriada);
    }

    public async Task<PecaDto> ObterPecaAsync(Guid id, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken);
        if (peca == null)
        {
            throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
        }

        return MapToDto(peca);
    }

    public async Task<IEnumerable<PecaDto>> ListarPecasAsync(CancellationToken cancellationToken)
    {
        var pecas = await _pecaRepository.ObterTodosAsync(cancellationToken);
        return pecas.Select(MapToDto);
    }

    public async Task<PecaDto> AtualizarPecaAsync(Guid id, AtualizarPecaDto dto, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken);
        if (peca == null)
        {
            throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
        }

        peca.Nome = dto.Nome;
        peca.Preco = dto.Preco;
        peca.QuantidadeEstoque = dto.QuantidadeEstoque;

        var pecaAtualizada = await _pecaRepository.AtualizarAsync(peca, cancellationToken);
        return MapToDto(pecaAtualizada);
    }

    public async Task DeletarPecaAsync(Guid id, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken);
        if (peca == null)
        {
            throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
        }

        await _pecaRepository.DeletarAsync(id, cancellationToken);
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
