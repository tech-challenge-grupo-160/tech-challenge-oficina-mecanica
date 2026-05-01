using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;

public static class ClienteMock
{
    public static Cliente Criar(
        int id = 1,
        string cpfCnpj = "47654866801",
        string nome = "Cliente Teste",
        string telefone = "11988887777",
        string email = "cliente@teste.com")
    {
        return new Cliente
        {
            Id = id,
            Nome = nome,
            CpfCnpj = cpfCnpj,
            Telefone = telefone,
            Email = email,
            DataCadastro = DateTime.UtcNow
        };
    }
}
