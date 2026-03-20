using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;

namespace oficina_mecanica.Application.Services;

public interface IClienteApplicationService
{
    Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto);
    Task<ClienteDto> ObterClienteAsync(Guid id);
    Task<ClienteDto> ObterClientePorCpfCnpjAsync(string cpfCnpj);
    Task<IEnumerable<ClienteDto>> ListarClientesAsync();
    Task<ClienteDto> AtualizarClienteAsync(Guid id, AtualizarClienteDto dto);
    Task DeletarClienteAsync(Guid id);
}

public class ClienteApplicationService : IClienteApplicationService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteApplicationService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto)
    {
        var clienteExistente = await _clienteRepository.ObterPorCpfCnpjAsync(dto.CpfCnpj);
        if (clienteExistente != null)
        {
            throw new InvalidOperationException("Cliente com este CPF/CNPJ já existe.");
        }

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            CpfCnpj = dto.CpfCnpj,
            Telefone = dto.Telefone,
            Email = dto.Email,
            DataCadastro = DateTime.UtcNow
        };

        var clienteCriado = await _clienteRepository.CriarAsync(cliente);
        return MapToDto(clienteCriado);
    }

    public async Task<ClienteDto> ObterClienteAsync(Guid id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} não encontrado.");
        }

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> ObterClientePorCpfCnpjAsync(string cpfCnpj)
    {
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(cpfCnpj);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} não encontrado.");
        }

        return MapToDto(cliente);
    }

    public async Task<IEnumerable<ClienteDto>> ListarClientesAsync()
    {
        var clientes = await _clienteRepository.ObterTodosAsync();
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto> AtualizarClienteAsync(Guid id, AtualizarClienteDto dto)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} não encontrado.");
        }

        cliente.Nome = dto.Nome;
        cliente.Telefone = dto.Telefone;
        cliente.Email = dto.Email;

        var clienteAtualizado = await _clienteRepository.AtualizarAsync(cliente);
        return MapToDto(clienteAtualizado);
    }

    public async Task DeletarClienteAsync(Guid id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} não encontrado.");
        }

        await _clienteRepository.DeletarAsync(id);
    }

    private static ClienteDto MapToDto(Cliente cliente)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            DataCadastro = cliente.DataCadastro
        };
    }
}
