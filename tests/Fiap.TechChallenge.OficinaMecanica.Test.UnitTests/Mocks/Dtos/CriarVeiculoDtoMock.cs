using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.DTOs;

public static class CriarVeiculoDtoMock
{
    public static CriarVeiculoDto Criar(
        string placa = "ABC1234",
        string marca = "Fiat",
        string modelo = "Uno",
        int ano = 2015,
        string cpfCnpj = "47654866801")
    {
        return new CriarVeiculoDto
        {
            Placa = placa,
            Marca = marca,
            Modelo = modelo,
            Ano = ano,
            CpfCnpj = cpfCnpj
        };
    }
}
