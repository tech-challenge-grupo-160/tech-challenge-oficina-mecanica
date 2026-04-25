using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;


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
            Nome = "Difusor de Óleos Essenciais Ultrassônico",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 189.90,
            QuantidadeEstoque = 15
        };
        
        var response = await _client.PostAsJsonAsync("/api/v1/pecas", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CriarPeca_deveRetornarBadRequest_quandoPrecoNegativo()
    {
        var body = new
        {
            Nome = "Radiador",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = -100,
            QuantidadeEstoque = 15
        };
        
        var response = await _client.PostAsJsonAsync("/api/v1/pecas", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task CriarPeca_deveRetornarBadRequest_quandoQuantidadeEstoqueZero()
    {
        var body = new
        {
            Nome = "Espelho Esportivo",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 192,
            QuantidadeEstoque = 0
        };
        
        var response = await _client.PostAsJsonAsync("/api/v1/pecas", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task CriarPeca_deveRetornarBadRequest_quandoNomeVazio()
    {
        var body = new
        {
            Nome = "",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 189.90,
            QuantidadeEstoque = 0
        };
        
        var response = await _client.PostAsJsonAsync("/api/v1/pecas", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ObterPeca_deveRetornarExistente()
    {
        var body = new
        {
            Nome = "Escapamento",
            Marca = "BioArno",
            Modelo = "Zen-04",
            Preco = 30,
            QuantidadeEstoque = 0
        };
        
        var responseCreatePeca = await _client.PostAsJsonAsync("/api/v1/pecas", body);
        var pecaCreated = responseCreatePeca.Content.ReadFromJsonAsync<PecaDto>();

        string idPeca = pecaCreated.Id.ToString();
        var responseListarPeca = await _client.GetAsync($"/api/v1/pecas/{idPeca}");
        PecaDto pecaListada = responseListarPeca.Content.ReadFromJsonAsync<PecaDto>().Result;
        
        pecaListada.Should().BeEquivalentTo(pecaListada);
    }

    [Fact]
    public async Task listarPecas_deveRetornarOk()
    {
        var responseListaPecas = await _client.GetAsync("/api/v1/pecas");
        IEnumerable<PecaDto> pecas = responseListaPecas.Content.ReadFromJsonAsync<IEnumerable<PecaDto>>().Result;
        pecas.Should().NotBeNull();
    }

    [Fact]
    public async Task Atualiza_deveAtualizarPeca()
    {
        var body = new
        {
            Nome = "Válvula",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 410,
            QuantidadeEstoque = 1
        };
        
        var responsePecaCreated = await _client.PostAsJsonAsync("/api/v1/pecas/", body);
        
        PecaDto pecaCreated = responsePecaCreated.Content.ReadFromJsonAsync<PecaDto>().Result;

        var bodyToUpdate = new
        {
            Nome = "Válvula",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 320,
            QuantidadeEstoque = 5
        };
        
        var responsePecaUpdated = await _client.PutAsJsonAsync($"/api/v1/pecas/{pecaCreated.Id}", bodyToUpdate);
        
        responsePecaUpdated.StatusCode.Should().Be(HttpStatusCode.OK);
        
        PecaDto pecaUpdated = responsePecaUpdated.Content.ReadFromJsonAsync<PecaDto>().Result;
        
        pecaUpdated.Should().NotBeEquivalentTo(pecaCreated);
        pecaUpdated.Id.Should().Be(pecaCreated.Id);
    }

    [Fact]
    public async Task delete_deveDeletarPeca()
    {
        var body = new
        {
            Nome = "Válvula",
            Marca = "BioArno",
            Modelo = "Zen-01",
            Preco = 410,
            QuantidadeEstoque = 1
        };
        
        var responsePecaCreated = await _client.PostAsJsonAsync("/api/v1/pecas/", body);
        
        PecaDto pecaCreated = responsePecaCreated.Content.ReadFromJsonAsync<PecaDto>().Result;

        var responseDelete = await _client.DeleteAsync($"/api/v1/Pecas/{pecaCreated.Id}");

        responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task delete_idInexistentedeveRetornarBadRequest()
    {
        var responseDelete = await _client.DeleteAsync($"/api/v1/Pecas/99999994666");

        responseDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}