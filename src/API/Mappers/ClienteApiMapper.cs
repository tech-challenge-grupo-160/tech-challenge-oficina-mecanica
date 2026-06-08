using Fiap.TechChallenge.OficinaMecanica.API.Requests.Clientes;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class ClienteApiMapper
{
    public static CriarClienteCommand ToCommand(this CriarClienteRequest request)
    {
        return new CriarClienteCommand
        {
            Nome = request.Nome,
            CpfCnpj = request.CpfCnpj,
            Telefone = request.Telefone,
            Email = request.Email
        };
    }

    public static AtualizarClienteCommand ToCommand(this AtualizarClienteRequest request, string cpfCnpj)
    {
        return new AtualizarClienteCommand
        {
            CpfCnpj = cpfCnpj,
            Nome = request.Nome,
            Telefone = request.Telefone,
            Email = request.Email
        };
    }

    public static DeletarClienteCommand ToDeleteCommand(this string cpfCnpj)
    {
        return new DeletarClienteCommand
        {
            CpfCnpj = cpfCnpj
        };
    }

    public static ObterClientePorDocumentoQuery ToQueryByDocumento(this string cpfCnpj)
    {
        return new ObterClientePorDocumentoQuery
        {
            CpfCnpj = cpfCnpj
        };
    }

    public static ClienteResponse ToResponse(this ClienteResult result)
    {
        return new ClienteResponse
        {
            Id = result.Id,
            Nome = result.Nome,
            CpfCnpj = result.CpfCnpj,
            Telefone = result.Telefone,
            Email = result.Email,
            DataCadastro = result.DataCadastro
        };
    }

    public static PagedResponse<ClienteResponse> ToResponse(this PagedResultDto<ClienteResult> result)
    {
        return new PagedResponse<ClienteResponse>
        {
            Items = result.Items.Select(cliente => cliente.ToResponse()).ToArray(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }
}
