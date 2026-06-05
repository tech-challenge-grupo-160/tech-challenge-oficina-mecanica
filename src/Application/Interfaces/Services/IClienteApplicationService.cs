using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

public interface IClienteApplicationService
{
    Task<ClienteResult> CriarClienteAsync(CriarClienteCommand command, CancellationToken cancellationToken);
    Task<ClienteResult> ObterClienteAsync(int id, CancellationToken cancellationToken);
    Task<ClienteResult> ObterClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
    Task<PagedResultDto<ClienteResult>> ListarClientesAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken);
    Task<ClienteResult> AtualizarClientePorCpfCnpjAsync(string cpfCnpj, AtualizarClienteCommand command, CancellationToken cancellationToken);
    Task DeletarClientePorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
}
