using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Acompanhamento;

public class AcompanhamentoOSControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AcompanhamentoOSControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ObterStatus_DeveRetornarProgressoQuandoCodigoETokenForemValidos()
    {
        _factory.ResetDatabase();
        var ordemCriada = await CriarOrdemAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/acompanhamento-os/{ordemCriada.CodigoAcompanhamento}");
        request.Headers.TryAddWithoutValidation("X-Tracking-Token", ordemCriada.TokenAcompanhamento);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AcompanhamentoOrdemDeServicoResponse>();
        body.Should().NotBeNull();
        body!.Numero.Should().Be(ordemCriada.Numero);
        body.CodigoAcompanhamento.Should().Be(ordemCriada.CodigoAcompanhamento);
        body.Status.Should().Be("Recebida");
        body.DataUltimaAtualizacao.Should().BeOnOrAfter(body.DataAbertura);
    }

    [Fact]
    public async Task ObterStatus_DeveRetornarNotFoundQuandoTokenForInvalido()
    {
        _factory.ResetDatabase();
        var ordemCriada = await CriarOrdemAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/acompanhamento-os/{ordemCriada.CodigoAcompanhamento}");
        request.Headers.TryAddWithoutValidation("X-Tracking-Token", "TOKEN-INVALIDO");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        await using var body = await response.Content.ReadAsStreamAsync();
        using var problemDetails = await JsonDocument.ParseAsync(body);
        var root = problemDetails.RootElement;

        root.GetProperty("status").GetInt32().Should().Be((int)HttpStatusCode.NotFound);
        root.GetProperty("title").GetString().Should().Be("Recurso nao encontrado.");
        root.GetProperty("detail").GetString().Should().Contain("Acompanhamento nao encontrado");
        root.GetProperty("instance").GetString().Should().Be($"/api/v1/acompanhamento-os/{ordemCriada.CodigoAcompanhamento}");
        root.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ObterStatus_DataUltimaAtualizacao_DeveMudarSomenteQuandoStatusMudar()
    {
        _factory.ResetDatabase();
        var ordemCriada = await CriarOrdemAsync();

        var acompanhamentoInicial = await ObterAcompanhamentoAsync(ordemCriada.CodigoAcompanhamento, ordemCriada.TokenAcompanhamento!);
        var dataInicial = acompanhamentoInicial.DataUltimaAtualizacao;

        var iniciarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        iniciarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);

        var acompanhamentoAposMudancaStatus = await ObterAcompanhamentoAsync(ordemCriada.CodigoAcompanhamento, ordemCriada.TokenAcompanhamento!);
        acompanhamentoAposMudancaStatus.DataUltimaAtualizacao.Should().BeAfter(dataInicial);
        var dataAposMudancaStatus = acompanhamentoAposMudancaStatus.DataUltimaAtualizacao;

        var adicionarServico = await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/servicos",
            new { servicoId = CustomWebApplicationFactory.ServicoExistenteId });
        adicionarServico.StatusCode.Should().Be(HttpStatusCode.OK);

        var acompanhamentoAposEventoSemMudancaStatus = await ObterAcompanhamentoAsync(ordemCriada.CodigoAcompanhamento, ordemCriada.TokenAcompanhamento!);
        acompanhamentoAposEventoSemMudancaStatus.DataUltimaAtualizacao.Should().Be(dataAposMudancaStatus);
    }

    private async Task<OrdemDeServicoResponse> CriarOrdemAsync()
    {
        var payload = new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Cliente relatou barulho na suspensao.",
            observacoesRecepcao = "Validar alinhamento e folgas."
        };

        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.CodigoAcompanhamento.Should().NotBeNullOrWhiteSpace();
        ordem.TokenAcompanhamento.Should().NotBeNullOrWhiteSpace();
        return ordem;
    }

    private async Task<AcompanhamentoOrdemDeServicoResponse> ObterAcompanhamentoAsync(string codigo, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/acompanhamento-os/{codigo}");
        request.Headers.TryAddWithoutValidation("X-Tracking-Token", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AcompanhamentoOrdemDeServicoResponse>();
        body.Should().NotBeNull();
        return body!;
    }
}
