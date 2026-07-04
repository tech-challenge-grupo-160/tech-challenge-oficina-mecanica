using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Servicos;

public class ServicosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ServicosControllerTests(CustomWebApplicationFactory factory)
    {
        factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarServico_DeveRetornarCreated()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/servicos",
            new
            {
                nome = "Balanceamento",
                descricao = "Balanceamento das rodas",
                preco = 120m,
                tempoEstimado = 45
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var servico = await response.Content.ReadFromJsonAsync<ServicoResponse>();
        servico.Should().NotBeNull();
        servico!.Id.Should().BeGreaterThan(0);
        servico.Nome.Should().Be("Balanceamento");
        servico.Preco.Should().Be(120m);
        servico.TempoEstimado.Should().Be(45);
    }

    [Fact]
    public async Task ObterServico_DeveRetornarServicoExistente()
    {
        var response = await _client.GetAsync($"/api/v1/servicos/{CustomWebApplicationFactory.ServicoExistenteId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var servico = await response.Content.ReadFromJsonAsync<ServicoResponse>();
        servico.Should().NotBeNull();
        servico!.Id.Should().Be(CustomWebApplicationFactory.ServicoExistenteId);
        servico.Nome.Should().Be("Alinhamento");
    }

    [Fact]
    public async Task ListarServicos_DeveRetornarOk()
    {
        var response = await _client.GetAsync("/api/v1/servicos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var servicos = await response.Content.ReadFromJsonAsync<List<ServicoResponse>>();
        servicos.Should().NotBeNull();
        servicos.Should().Contain(x => x.Id == CustomWebApplicationFactory.ServicoExistenteId);
    }

    [Fact]
    public async Task AtualizarServico_DeveRetornarServicoAtualizado()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/servicos/{CustomWebApplicationFactory.ServicoExistenteId}",
            new
            {
                nome = "Alinhamento completo",
                descricao = "Alinhamento e cambagem",
                preco = 220m,
                tempoEstimado = 60
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var servico = await response.Content.ReadFromJsonAsync<ServicoResponse>();
        servico.Should().NotBeNull();
        servico!.Id.Should().Be(CustomWebApplicationFactory.ServicoExistenteId);
        servico.Nome.Should().Be("Alinhamento completo");
        servico.Descricao.Should().Be("Alinhamento e cambagem");
        servico.Preco.Should().Be(220m);
        servico.TempoEstimado.Should().Be(60);
    }

    [Fact]
    public async Task DeletarServico_DeveRetornarNoContentQuandoSemOrdensAtivas()
    {
        var create = await _client.PostAsJsonAsync(
            "/api/v1/servicos",
            new
            {
                nome = "Higienizacao",
                descricao = "Higienizacao interna",
                preco = 180m,
                tempoEstimado = 90
            });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var servico = await create.Content.ReadFromJsonAsync<ServicoResponse>();

        var response = await _client.DeleteAsync($"/api/v1/servicos/{servico!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ObterServico_DeveRetornarNotFoundProblemDetailsQuandoServicoNaoExistir()
    {
        var response = await _client.GetAsync("/api/v1/servicos/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        await using var body = await response.Content.ReadAsStreamAsync();
        using var problemDetails = await JsonDocument.ParseAsync(body);
        var root = problemDetails.RootElement;

        root.GetProperty("status").GetInt32().Should().Be((int)HttpStatusCode.NotFound);
        root.GetProperty("detail").GetString().Should().Contain("Servico com ID 999999 nao encontrado");
        root.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
