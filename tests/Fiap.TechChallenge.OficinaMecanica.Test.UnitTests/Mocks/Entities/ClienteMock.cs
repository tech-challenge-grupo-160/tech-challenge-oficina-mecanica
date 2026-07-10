using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

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
        return Cliente.Criar(nome, Documento.Parse(cpfCnpj), Telefone.Parse(telefone), Email.Parse(email), DateTime.UtcNow)
            .WithId(id);
    }
}
