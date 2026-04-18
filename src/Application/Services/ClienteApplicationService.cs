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
    Task<PagedResultDto<ClienteDto>> ListarClientesAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken);
    Task<ClienteDto> AtualizarClientePorCpfCnpjAsync(string cpfCnpj, AtualizarClienteDto dto, CancellationToken cancellationToken);
    Task DeletarClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
}

public class ClienteApplicationService : IClienteApplicationService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;

    public ClienteApplicationService(
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IOrdemDeServicoRepository ordemDeServicoRepository)
    {
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
    }

    public async Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto, CancellationToken cancellationToken)
    {
        var documento = DocumentoHelper.NormalizarDocumento(dto.CpfCnpj);
        var telefone = TelefoneHelper.Normalizar(dto.Telefone);
        var clienteExistente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (clienteExistente != null)
        {
            throw new InvalidOperationException("Cliente com este CPF/CNPJ ja existe.");
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
            throw new KeyNotFoundException($"Cliente com ID {id} nao encontrado.");
        }

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> ObterClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
        }

        return MapToDto(cliente);
    }

    public async Task<PagedResultDto<ClienteDto>> ListarClientesAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken)
    {
        var documentoFiltro = NormalizarDocumentoParaBusca(cpfCnpj);

        var nomeFiltro = string.IsNullOrWhiteSpace(nome)
            ? null
            : nome.Trim();

        var totalItems = await _clienteRepository.ContarAsync(nomeFiltro, documentoFiltro, cancellationToken);
        var clientes = await _clienteRepository.ObterPaginadoAsync(page, pageSize, nomeFiltro, documentoFiltro, cancellationToken);

        return new PagedResultDto<ClienteDto>
        {
            Items = clientes.Select(MapToDto).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ClienteDto> AtualizarClientePorCpfCnpjAsync(string cpfCnpj, AtualizarClienteDto dto, CancellationToken cancellationToken)
    {
        var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
        }

        cliente.Nome = dto.Nome;
        cliente.Telefone = TelefoneHelper.Normalizar(dto.Telefone);
        cliente.Email = dto.Email;

        var clienteAtualizado = await _clienteRepository.AtualizarAsync(cliente, cancellationToken);
        return MapToDto(clienteAtualizado);
    }

    public async Task DeletarClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        var documento = DocumentoHelper.NormalizarDocumento(cpfCnpj);
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new KeyNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
        }

        if (await _veiculoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
        {
            throw new InvalidOperationException("Nao e possivel excluir o cliente, pois existem veiculos vinculados.");
        }

        if (await _ordemDeServicoRepository.ExistePorClienteAsync(cliente.Id, cancellationToken))
        {
            throw new InvalidOperationException("Nao e possivel excluir o cliente, pois existem ordens de servico vinculadas.");
        }

        await _clienteRepository.DeletarAsync(cliente.Id, cancellationToken);
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

    private static string? NormalizarDocumentoParaBusca(string? cpfCnpj)
    {
        if (string.IsNullOrWhiteSpace(cpfCnpj))
        {
            return null;
        }

        var normalizado = new string(cpfCnpj
            .Trim()
            .Where(c => char.IsDigit(c) || char.IsLetter(c))
            .Select(char.ToUpperInvariant)
            .ToArray());

        return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
    }
}
