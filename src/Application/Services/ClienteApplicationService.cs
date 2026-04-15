using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IClienteApplicationService
{
    Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto, CancellationToken cancellationToken);
    Task<ClienteDto> ObterClienteAsync(int id, CancellationToken cancellationToken);
    Task<ClienteDto> ObterClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
    Task<IEnumerable<ClienteDto>> ListarClientesAsync(CancellationToken cancellationToken);
    Task<ClienteDto> AtualizarClienteAsync(int id, AtualizarClienteDto dto, CancellationToken cancellationToken);
    Task DeletarClienteAsync(int id, CancellationToken cancellationToken);
}

public class ClienteApplicationService : IClienteApplicationService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteApplicationService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto, CancellationToken cancellationToken)
    {
        var documento = DocumentoHelper.NormalizarDocumento(dto.CpfCnpj);
        var telefone = TelefoneHelper.Normalizar(dto.Telefone);
        var clienteExistente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (clienteExistente != null)
        {
            throw new InvalidOperationException("Cliente com este CPF/CNPJ já existe.");
        }

        var cliente = new Cliente
        {
            Nome = dto.Nome,
            CpfCnpj = documento,
            Telefone = telefone,
            Email = dto.Email,
            DataCadastro = DateTimeHelper.UTCBrazilNow()
        };

        var clienteCriado = await _clienteRepository.CriarAsync(cliente, cancellationToken);
        return MapToDto(clienteCriado);
    }

    public async Task<ClienteDto> ObterClienteAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} não encontrado.");
        }

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> ObterClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} não encontrado.");
        }

        return MapToDto(cliente);
    }

    public async Task<IEnumerable<ClienteDto>> ListarClientesAsync(CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.ObterTodosAsync(cancellationToken);
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto> AtualizarClienteAsync(int id, AtualizarClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} não encontrado.");
        }

        cliente.Nome = dto.Nome;
        cliente.Telefone = TelefoneHelper.Normalizar(dto.Telefone);
        cliente.Email = dto.Email;

        var clienteAtualizado = await _clienteRepository.AtualizarAsync(cliente, cancellationToken);
        return MapToDto(clienteAtualizado);
    }

    public async Task DeletarClienteAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} não encontrado.");
        }

        await _clienteRepository.DeletarAsync(id, cancellationToken);
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
