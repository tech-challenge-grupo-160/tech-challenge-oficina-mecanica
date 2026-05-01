using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Veiculos;

public class VeiculosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VeiculosControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ObterPorPlaca_DeveRetornarVeiculoExistente()
    {
        var response = await _client.GetAsync("/api/v1/Veiculos/placa/bra-2e19");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var veiculo = await response.Content.ReadFromJsonAsync<VeiculoDto>();
        veiculo.Should().NotBeNull();
        veiculo!.Placa.Should().Be("BRA2E19");
        veiculo.ClienteId.Should().Be(CustomWebApplicationFactory.PessoaFisicaClienteId);
    }

    [Fact]
    public async Task Criar_DeveRetornarCreatedQuandoPayloadValido()
    {
        var payload = new
        {
            placa = "abc-1234",
            marca = "Fiat",
            modelo = "Uno",
            ano = 2018,
            cpfCnpj = "60617051000199"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Veiculos", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var veiculo = await response.Content.ReadFromJsonAsync<VeiculoDto>();
        veiculo.Should().NotBeNull();
        veiculo!.Placa.Should().Be("ABC1234");
        veiculo.ClienteId.Should().Be(CustomWebApplicationFactory.PessoaJuridicaClienteId);
    }

    [Fact]
    public async Task Criar_DeveRetornarBadRequestQuandoPlacaForInvalida()
    {
        var payload = new
        {
            placa = "1234567",
            marca = "Fiat",
            modelo = "Uno",
            ano = 2018,
            cpfCnpj = "47654866801"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Veiculos", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
