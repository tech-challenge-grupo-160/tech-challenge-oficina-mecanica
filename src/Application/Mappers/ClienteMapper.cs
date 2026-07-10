using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Mappers;

public static class ClienteMapper
{
    public static ClienteResult ToResult(this Cliente cliente)
    {
        return new ClienteResult
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj.Valor,
            Telefone = cliente.Telefone.Valor,
            Email = cliente.Email.Valor,
            DataCadastro = cliente.DataCadastro
        };
    }
}
