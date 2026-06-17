using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Commands;

public static class CriarClienteCommandMock
{
    public static CriarClienteCommand Criar(
        string nome = "Cliente Teste",
        string cpfCnpj = "47654866801",
        string email = "cliente@teste.com",
        string telefone = "11999999999")
    {
        return new CriarClienteCommand
        {
            Nome = nome,
            CpfCnpj = cpfCnpj,
            Email = email,
            Telefone = telefone
        };
    }
}
