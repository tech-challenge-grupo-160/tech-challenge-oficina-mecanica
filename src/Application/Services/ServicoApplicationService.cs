using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;

namespace oficina_mecanica.Application.Services;

public interface IServicoApplicationService
{
    Task<ServicoDto> CriarServicoAsync(CriarServicoDto dto);
    Task<ServicoDto> ObterServicoAsync(Guid id);
    Task<IEnumerable<ServicoDto>> ListarServicosAsync();
    Task<ServicoDto> AtualizarServicoAsync(Guid id, AtualizarServicoDto dto);
    Task DeletarServicoAsync(Guid id);
}

public class ServicoApplicationService : IServicoApplicationService
{
    private readonly IServicoRepository _servicoRepository;

    public ServicoApplicationService(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<ServicoDto> CriarServicoAsync(CriarServicoDto dto)
    {
        var servico = new Servico
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            TempoEstimado = dto.TempoEstimado
        };

        var servicoCriado = await _servicoRepository.CriarAsync(servico);
        return MapToDto(servicoCriado);
    }

    public async Task<ServicoDto> ObterServicoAsync(Guid id)
    {
        var servico = await _servicoRepository.ObterPorIdAsync(id);
        if (servico == null)
        {
            throw new KeyNotFoundException($"Serviço com ID {id} não encontrado.");
        }

        return MapToDto(servico);
    }

    public async Task<IEnumerable<ServicoDto>> ListarServicosAsync()
    {
        var servicos = await _servicoRepository.ObterTodosAsync();
        return servicos.Select(MapToDto);
    }

    public async Task<ServicoDto> AtualizarServicoAsync(Guid id, AtualizarServicoDto dto)
    {
        var servico = await _servicoRepository.ObterPorIdAsync(id);
        if (servico == null)
        {
            throw new KeyNotFoundException($"Serviço com ID {id} não encontrado.");
        }

        servico.Nome = dto.Nome;
        servico.Descricao = dto.Descricao;
        servico.Preco = dto.Preco;
        servico.TempoEstimado = dto.TempoEstimado;

        var servicoAtualizado = await _servicoRepository.AtualizarAsync(servico);
        return MapToDto(servicoAtualizado);
    }

    public async Task DeletarServicoAsync(Guid id)
    {
        var servico = await _servicoRepository.ObterPorIdAsync(id);
        if (servico == null)
        {
            throw new KeyNotFoundException($"Serviço com ID {id} não encontrado.");
        }

        await _servicoRepository.DeletarAsync(id);
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
