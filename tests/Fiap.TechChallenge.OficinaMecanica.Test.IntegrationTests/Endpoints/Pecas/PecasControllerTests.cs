using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Pecas;

public class PecasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PecasControllerTests(CustomWebApplicationFactory factory)
    {
        factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarPeca_deveRetornarCreated()
    {
        var body = new
        {
            Nome = "Difusor de Oleos Essenciais Ultrassonico",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 189.90,
            QuantidadeEstoque = 15
        };

        var response = await _client.PostAsJsonAsync("/api/v1/pecas", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task listarPecas_deveRetornarOk()
    {
        var responseListaPecas = await _client.GetAsync("/api/v1/pecas");
        var pecas = await responseListaPecas.Content.ReadFromJsonAsync<IEnumerable<PecaResponse>>();
        pecas.Should().NotBeNull();
    }

    [Fact]
    public async Task Atualiza_deveAtualizarPeca()
    {
        var body = new
        {
            Nome = "Valvula",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 410,
            QuantidadeEstoque = 1
        };

        var responsePecaCreated = await _client.PostAsJsonAsync("/api/v1/pecas/", body);
        var pecaCreated = await responsePecaCreated.Content.ReadFromJsonAsync<PecaResponse>();

        var bodyToUpdate = new
        {
            Nome = "Valvula",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 320,
            QuantidadeEstoque = 5
        };

        var responsePecaUpdated = await _client.PutAsJsonAsync($"/api/v1/pecas/{pecaCreated!.Id}", bodyToUpdate);

        responsePecaUpdated.StatusCode.Should().Be(HttpStatusCode.OK);

        var pecaUpdated = await responsePecaUpdated.Content.ReadFromJsonAsync<PecaResponse>();

        pecaUpdated.Should().NotBeNull();
        pecaUpdated.Should().NotBeEquivalentTo(pecaCreated);
        pecaUpdated!.Id.Should().Be(pecaCreated.Id);
    }

    [Fact]
    public async Task delete_deveDeletarPeca()
    {
        var body = new
        {
            Nome = "Valvula",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 410,
            QuantidadeEstoque = 1
        };

        var responsePecaCreated = await _client.PostAsJsonAsync("/api/v1/pecas/", body);
        var pecaCreated = await responsePecaCreated.Content.ReadFromJsonAsync<PecaResponse>();

        var responseDelete = await _client.DeleteAsync($"/api/v1/Pecas/{pecaCreated!.Id}");

        responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task delete_idInexistentedeveRetornarBadRequest()
    {
        var responseDelete = await _client.DeleteAsync("/api/v1/Pecas/99999994666");

        responseDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
