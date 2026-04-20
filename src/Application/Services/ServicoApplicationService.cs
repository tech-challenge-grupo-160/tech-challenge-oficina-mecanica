using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

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
    private readonly IServicoRepository _servicoRepository;

    public ServicoApplicationService(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<ServicoDto> CriarServicoAsync(CriarServicoDto dto, CancellationToken cancellationToken)
    {
        var servico = new Servico
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            TempoEstimado = dto.TempoEstimado
        };

        var servicoCriado = await _servicoRepository.CriarAsync(servico, cancellationToken);
        return MapToDto(servicoCriado);
    }

    public async Task<ServicoDto> ObterServicoAsync(int id, CancellationToken cancellationToken)
    {
        var servico = await _servicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (servico == null)
        {
            throw new KeyNotFoundException($"Serviço com ID {id} não encontrado.");
        }

        return MapToDto(servico);
    }

    public async Task<IEnumerable<ServicoDto>> ListarServicosAsync(CancellationToken cancellationToken)
    {
        var servicos = await _servicoRepository.ObterTodosAsync(cancellationToken);
        return servicos.Select(MapToDto);
    }

    public async Task<ServicoDto> AtualizarServicoAsync(int id, AtualizarServicoDto dto, CancellationToken cancellationToken)
    {
        var servico = await _servicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (servico == null)
        {
            throw new KeyNotFoundException($"Serviço com ID {id} não encontrado.");
        }

        servico.Nome = dto.Nome;
        servico.Descricao = dto.Descricao;
        servico.Preco = dto.Preco;
        servico.TempoEstimado = dto.TempoEstimado;

        var servicoAtualizado = await _servicoRepository.AtualizarAsync(servico, cancellationToken);
        return MapToDto(servicoAtualizado);
    }

    public async Task DeletarServicoAsync(int id, CancellationToken cancellationToken)
    {
        var servico = await _servicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (servico == null)
        {
            throw new KeyNotFoundException($"Serviço com ID {id} não encontrado.");
        }

        await _servicoRepository.DeletarAsync(id, cancellationToken);
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
