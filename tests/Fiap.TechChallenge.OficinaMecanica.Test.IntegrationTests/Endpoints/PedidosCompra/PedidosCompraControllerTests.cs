using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.PedidosCompra;

public class PedidosCompraControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PedidosCompraControllerTests(CustomWebApplicationFactory factory)
    {
        factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Listar_DeveRetornarPedidosDeCompraPaginados()
    {
        var primeiraOrdem = await CriarOrdemComPedidoCompraAsync(
            "Barulho no freio dianteiro.",
            CustomWebApplicationFactory.PessoaFisicaClienteId,
            CustomWebApplicationFactory.VeiculoExistenteId);
        var segundaOrdem = await CriarOrdemComPedidoCompraAsync(
            "Barulho no freio traseiro.",
            CustomWebApplicationFactory.PessoaJuridicaClienteId,
            CustomWebApplicationFactory.SegundoVeiculoExistenteId);

        var response = await _client.GetAsync("/api/v1/pedidos-compra?page=1&pageSize=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultado = await response.Content.ReadFromJsonAsync<PagedResponse<PedidoCompraResponse>>();
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(1);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(1);
        resultado.TotalItems.Should().Be(2);
        resultado.TotalPages.Should().Be(2);
        resultado.Items.Single().Status.Should().Be("Pendente");
        resultado.Items.Single().OrdemDeServicoId.Should().BeOneOf(primeiraOrdem.Id, segundaOrdem.Id);
    }

    [Fact]
    public async Task Criar_DevePermitirPedidoCompraManualSemGeracaoAutomatica()
    {
        var ordem = await CriarOrdemAsync(
            "Troca preventiva de insumos.",
            CustomWebApplicationFactory.PessoaFisicaClienteId,
            CustomWebApplicationFactory.VeiculoExistenteId);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/pedidos-compra",
            new
            {
                ordemDeServicoId = ordem.Id,
                pecaId = CustomWebApplicationFactory.PecaExistenteId,
                quantidadeSolicitada = 4,
                observacao = "Pedido manual para reposicao"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var pedido = await response.Content.ReadFromJsonAsync<PedidoCompraResponse>();
        pedido.Should().NotBeNull();
        pedido!.OrdemDeServicoId.Should().Be(ordem.Id);
        pedido.PecaId.Should().Be(CustomWebApplicationFactory.PecaExistenteId);
        pedido.QuantidadeSolicitada.Should().Be(4);
        pedido.Status.Should().Be("Pendente");
        pedido.Observacao.Should().Be("Pedido manual para reposicao");
    }

    private async Task<OrdemDeServicoResponse> CriarOrdemComPedidoCompraAsync(string descricaoSolicitacao, int clienteId, int veiculoId)
    {
        var ordem = await CriarOrdemAsync(descricaoSolicitacao, clienteId, veiculoId);

        var iniciar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);
        iniciar.StatusCode.Should().Be(HttpStatusCode.OK);

        var adicionarPeca = await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordem.Id}/pecas",
            new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 101 });
        adicionarPeca.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalizar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordem.Id}/finalizar-diagnostico", null);
        finalizar.StatusCode.Should().Be(HttpStatusCode.OK);

        var aprovarResponse = await _client.PatchAsync($"/api/v1/ordens-servico/{ordem.Id}/aprovar", null);
        aprovarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var osBloqueada = await aprovarResponse.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        osBloqueada.Should().NotBeNull();
        osBloqueada!.Status.Should().Be("AguardandoEstoque");

        return ordem;
    }

    private async Task<OrdemDeServicoResponse> CriarOrdemAsync(string descricaoSolicitacao, int clienteId, int veiculoId)
    {
        var payload = new
        {
            clienteId,
            veiculoId,
            descricaoSolicitacao,
            observacoesRecepcao = "Pedido para gerar compra.",
            servicos = new[]
            {
                new { servicoId = CustomWebApplicationFactory.ServicoExistenteId }
            },
            pecas = Array.Empty<object>()
        };

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/ordens-servico", payload);
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var ordem = await criarResponse.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        return ordem!;
    }
}
