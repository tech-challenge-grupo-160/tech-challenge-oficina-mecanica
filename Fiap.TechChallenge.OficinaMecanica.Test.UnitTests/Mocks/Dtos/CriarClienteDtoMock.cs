using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.DTOs;

public static class CriarClienteDtoMock
{
    public static CriarClienteDto Criar(
        string nome = "Cliente Teste",
        string cpfCnpj = "47654866801",
        string email = "cliente@teste.com",
        string telefone = "11999999999")
    {
        return new CriarClienteDto
        {
            Nome = nome,
            CpfCnpj = cpfCnpj,
            Email = email,
            Telefone = telefone
        };
    }
}
