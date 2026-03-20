using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;

namespace oficina_mecanica.Application.Services;

public interface IPecaApplicationService
{
    Task<PecaDto> CriarPecaAsync(CriarPecaDto dto);
    Task<PecaDto> ObterPecaAsync(Guid id);
    Task<IEnumerable<PecaDto>> ListarPecasAsync();
    Task<PecaDto> AtualizarPecaAsync(Guid id, AtualizarPecaDto dto);
    Task DeletarPecaAsync(Guid id);
}

public class PecaApplicationService : IPecaApplicationService
{
    private readonly IPecaRepository _pecaRepository;

    public PecaApplicationService(IPecaRepository pecaRepository)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<PecaDto> CriarPecaAsync(CriarPecaDto dto)
    {
        var peca = new Peca
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Preco = dto.Preco,
            QuantidadeEstoque = dto.QuantidadeEstoque
        };

        var pecaCriada = await _pecaRepository.CriarAsync(peca);
        return MapToDto(pecaCriada);
    }

    public async Task<PecaDto> ObterPecaAsync(Guid id)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id);
        if (peca == null)
        {
            throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
        }

        return MapToDto(peca);
    }

    public async Task<IEnumerable<PecaDto>> ListarPecasAsync()
    {
        var pecas = await _pecaRepository.ObterTodosAsync();
        return pecas.Select(MapToDto);
    }

    public async Task<PecaDto> AtualizarPecaAsync(Guid id, AtualizarPecaDto dto)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id);
        if (peca == null)
        {
            throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
        }

        peca.Nome = dto.Nome;
        peca.Preco = dto.Preco;
        peca.QuantidadeEstoque = dto.QuantidadeEstoque;

        var pecaAtualizada = await _pecaRepository.AtualizarAsync(peca);
        return MapToDto(pecaAtualizada);
    }

    public async Task DeletarPecaAsync(Guid id)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id);
        if (peca == null)
        {
            throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
        }

        await _pecaRepository.DeletarAsync(id);
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
