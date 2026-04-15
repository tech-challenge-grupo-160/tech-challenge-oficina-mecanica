using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests;

public class ClientesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ClientesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ObterPorDocumento_DeveRetornarClienteExistente()
    {
        var response = await _client.GetAsync("/api/v1/Clientes/documento/47654866801");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteDto>();
        cliente.Should().NotBeNull();
        cliente!.Nome.Should().Be("Vanessa Luna Duarte");
        cliente.CpfCnpj.Should().Be("47654866801");
    }

    [Fact]
    public async Task ObterPorDocumento_DeveRetornarEmpresaQuandoCnpj()
    {
        var response = await _client.GetAsync("/api/v1/Clientes/documento/60617051000199");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteDto>();
        cliente.Should().NotBeNull();
        cliente!.Nome.Should().Be("Betina e Fernanda Contabil Ltda");
        cliente.CpfCnpj.Should().Be("60617051000199");
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarCreatedQuandoPayloadValido()
    {
        var payload = new
        {
            nome = "Cliente Integracao",
            cpfCnpj = "52998224725",
            email = "integracao@teste.com",
            telefone = "11988887777"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Clientes", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteDto>();
        cliente.Should().NotBeNull();
        cliente!.Nome.Should().Be("Cliente Integracao");
        cliente.CpfCnpj.Should().Be("52998224725");
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarBadRequestQuandoDadosObrigatoriosFaltarem()
    {
        var payload = new
        {
            cpfCnpj = "59362967063",
            telefone = "11987654321"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Clientes", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
