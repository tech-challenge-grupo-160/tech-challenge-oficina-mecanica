using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;

public static class VeiculoMock
{
    public static Veiculo Criar(
        int id = 1,
        string placa = "ABC1234",
        string marca = "Fiat",
        string modelo = "Uno",
        int ano = 2015,
        int clienteId = 1)
    {
        return new Veiculo
        {
            Id = id,
            Placa = placa,
            Marca = marca,
            Modelo = modelo,
            Ano = ano,
            ClienteId = clienteId
        };
    }
}
