using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.OrdensDeServico;

public class OrdensDeServicoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdensDeServicoControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FluxoCompleto_DevePercorrerCaminhoFelizDaOs()
    {
        var ordemCriada = await CriarOrdemAsync();

        var iniciarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        iniciarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);

        var adicionarServico = await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/servicos",
            new { servicoId = CustomWebApplicationFactory.ServicoExistenteId });
        adicionarServico.StatusCode.Should().Be(HttpStatusCode.OK);

        var adicionarPeca = await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/pecas",
            new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 1 });
        adicionarPeca.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalizarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);
        finalizarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);
        var aguardandoAprovacao = await finalizarDiagnostico.Content.ReadFromJsonAsync<OrdemDeServicoDto>();
        aguardandoAprovacao.Should().NotBeNull();
        aguardandoAprovacao!.Status.Should().Be("AguardandoAprovacao");
        aguardandoAprovacao.OrcamentoEnviadoEm.Should().NotBeNull();
        aguardandoAprovacao.ValorTotal.Should().Be(195m);

        var aprovar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);
        aprovar.StatusCode.Should().Be(HttpStatusCode.OK);
        var emExecucao = await aprovar.Content.ReadFromJsonAsync<OrdemDeServicoDto>();
        emExecucao!.Status.Should().Be("EmExecucao");

        var finalizar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar", null);
        finalizar.StatusCode.Should().Be(HttpStatusCode.OK);
        var aguardandoPagamento = await finalizar.Content.ReadFromJsonAsync<OrdemDeServicoDto>();
        aguardandoPagamento!.Status.Should().Be("AguardandoPagamento");
        aguardandoPagamento.DataFinalizacao.Should().NotBeNull();

        var registrarPagamento = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/registrar-pagamento", null);
        registrarPagamento.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagamentoRegistrado = await registrarPagamento.Content.ReadFromJsonAsync<OrdemDeServicoDto>();
        pagamentoRegistrado!.Status.Should().Be("AguardandoPagamento");
        pagamentoRegistrado.DataPagamento.Should().NotBeNull();

        var entregar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/entregar", null);
        entregar.StatusCode.Should().Be(HttpStatusCode.OK);
        var entregue = await entregar.Content.ReadFromJsonAsync<OrdemDeServicoDto>();
        entregue!.Status.Should().Be("Entregue");
        entregue.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task FinalizarDiagnostico_DeveRetornarBadRequestQuandoNaoHouverServico()
    {
        var ordemCriada = await CriarOrdemAsync();

        var iniciarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        iniciarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalizarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        finalizarDiagnostico.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await finalizarDiagnostico.Content.ReadAsStringAsync();
        body.Should().Contain("ao menos um servico");
    }

    [Fact]
    public async Task Entregar_DeveRetornarBadRequestQuandoPagamentoNaoTiverSidoRegistrado()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/servicos",
            new { servicoId = CustomWebApplicationFactory.ServicoExistenteId });
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar", null);

        var entregar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/entregar", null);

        entregar.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await entregar.Content.ReadAsStringAsync();
        body.Should().Contain("pagamento");
    }

    private async Task<OrdemDeServicoDto> CriarOrdemAsync()
    {
        var payload = new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Cliente relatou barulho ao frear e puxando para a direita.",
            observacoesRecepcao = "Problema ocorre em baixa velocidade."
        };

        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoDto>();
        ordem.Should().NotBeNull();
        return ordem!;
    }
}
