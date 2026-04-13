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
        cliente!.Nome.Should().Be("Betina e Fernanda Contábil Ltda");
        cliente.CpfCnpj.Should().Be("60617051000199");
    }
}
