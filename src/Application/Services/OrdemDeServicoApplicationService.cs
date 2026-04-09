using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;

namespace oficina_mecanica.Application.Services;

public interface IOrdemDeServicoApplicationService
{
    Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoAsync(CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorClienteAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorStatusAsync(string status, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> AtualizarStatusAsync(Guid id, AtualizarStatusOrdemDeServicoDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> AdicionarServicoAsync(Guid id, AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken);
    Task<OrdemDeServicoDto> AdicionarPecaAsync(Guid id, AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken);
    Task DeletarOrdemDeServicoAsync(Guid id, CancellationToken cancellationToken);
}

public class OrdemDeServicoApplicationService : IOrdemDeServicoApplicationService
{
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaRepository _pecaRepository;

    public OrdemDeServicoApplicationService(
        IOrdemDeServicoRepository ordemRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IPecaRepository pecaRepository)
    {
        _ordemRepository = ordemRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _pecaRepository = pecaRepository;
    }

    public async Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(dto.ClienteId, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");
        }

        var veiculo = await _veiculoRepository.ObterPorIdAsync(dto.VeiculoId, cancellationToken);
        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {dto.VeiculoId} não encontrado.");
        }

        var numero = GerarNumeroOrdem();
        var ordem = new OrdemDeServico
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            ClienteId = dto.ClienteId,
            VeiculoId = dto.VeiculoId,
            Status = StatusOrdemDeServico.Recebida,
            DataAbertura = DateTime.UtcNow,
            ValorTotal = 0
        };

        var ordemCriada = await _ordemRepository.CriarAsync(ordem, cancellationToken);
        return MapToDto(ordemCriada);
    }

    public async Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(Guid id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de serviço com ID {id} não encontrada.");
        }

        return MapToDto(ordem);
    }

    public async Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoAsync(CancellationToken cancellationToken)
    {
        var ordens = await _ordemRepository.ObterTodosAsync(cancellationToken);
        return ordens.Select(MapToDto);
    }

    public async Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorClienteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {clienteId} não encontrado.");
        }

        var ordens = await _ordemRepository.ObterPorClienteAsync(clienteId, cancellationToken);
        return ordens.Select(MapToDto);
    }

    public async Task<IEnumerable<OrdemDeServicoDto>> ListarOrdensDeServicoPorStatusAsync(string status, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<StatusOrdemDeServico>(status, out var statusEnum))
        {
            throw new InvalidOperationException($"Status inválido: {status}");
        }

        var ordens = await _ordemRepository.ObterPorStatusAsync(statusEnum, cancellationToken);
        return ordens.Select(MapToDto);
    }

    public async Task<OrdemDeServicoDto> AtualizarStatusAsync(Guid id, AtualizarStatusOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de serviço com ID {id} não encontrada.");
        }

        if (!Enum.TryParse<StatusOrdemDeServico>(dto.NovoStatus, out var novoStatus))
        {
            throw new InvalidOperationException($"Status inválido: {dto.NovoStatus}");
        }

        ordem.AlterarStatus(novoStatus);
        var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        return MapToDto(ordemAtualizada);
    }

    public async Task<OrdemDeServicoDto> AdicionarServicoAsync(Guid id, AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de serviço com ID {id} não encontrada.");
        }

        var servico = await _servicoRepository.ObterPorIdAsync(dto.ServicoId, cancellationToken);
        if (servico == null)
        {
            throw new KeyNotFoundException($"Serviço com ID {dto.ServicoId} não encontrado.");
        }

        ordem.AdicionarServico(servico);
        var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        return MapToDto(ordemAtualizada);
    }

    public async Task<OrdemDeServicoDto> AdicionarPecaAsync(Guid id, AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de serviço com ID {id} não encontrada.");
        }

        var peca = await _pecaRepository.ObterPorIdAsync(dto.PecaId, cancellationToken);
        if (peca == null)
        {
            throw new KeyNotFoundException($"Peça com ID {dto.PecaId} não encontrada.");
        }

        ordem.AdicionarPeca(peca, dto.Quantidade);
        var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        return MapToDto(ordemAtualizada);
    }

    public async Task DeletarOrdemDeServicoAsync(Guid id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de serviço com ID {id} não encontrada.");
        }

        await _ordemRepository.DeletarAsync(id, cancellationToken);
    }

    private static string GerarNumeroOrdem()
    {
        return $"OS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    private static OrdemDeServicoDto MapToDto(OrdemDeServico ordem)
    {
        return new OrdemDeServicoDto
        {
            Id = ordem.Id,
            Numero = ordem.Numero,
            ClienteId = ordem.ClienteId,
            VeiculoId = ordem.VeiculoId,
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
