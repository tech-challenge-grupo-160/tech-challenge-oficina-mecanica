using System.Net;
using System.Net.Http.Json;
using System.Text;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.OrdensDeServico;

public class OrdensDeServicoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdensDeServicoControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarConsultarEListar_DevePreservarContratoDaOs()
    {
        var ordemCriada = await CriarOrdemAsync();

        var obter = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}");
        var listar = await _client.GetAsync("/api/v1/ordens-servico?page=1&pageSize=10");
        var historicoResponse = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/historico");
        var notificacoesResponse = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/notificacoes");

        obter.StatusCode.Should().Be(HttpStatusCode.OK);
        listar.StatusCode.Should().Be(HttpStatusCode.OK);
        historicoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        notificacoesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var ordemConsultada = await obter.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordemConsultada.Should().NotBeNull();
        ordemConsultada!.Id.Should().Be(ordemCriada.Id);
        ordemConsultada.Status.Should().Be(ordemCriada.Status);
        ordemConsultada.Servicos.Should().BeEquivalentTo(ordemCriada.Servicos);
        ordemConsultada.Pecas.Should().BeEquivalentTo(ordemCriada.Pecas);

        var pagina = await listar.Content.ReadFromJsonAsync<PagedResponse<OrdemDeServicoResponse>>();
        pagina.Should().NotBeNull();
        pagina!.Items.Should().ContainSingle(item => item.Id == ordemCriada.Id);

        var historico = await historicoResponse.Content.ReadFromJsonAsync<List<OrdemServicoHistoricoResponse>>();
        historico.Should().Contain(item => item.TipoEvento == "OrdemCriada");
        historico.Should().Contain(item => item.TipoEvento == "ServicoAdicionado");

        var notificacoes = await notificacoesResponse.Content.ReadFromJsonAsync<List<NotificacaoClienteResponse>>();
        notificacoes.Should().Contain(item => item.TipoNotificacao == "LinkAcompanhamentoEnviado");
    }

    [Fact]
    public async Task CriarOrdem_DeveRetornarBadRequestQuandoJaExistirOsAtivaParaMesmoClienteEVeiculo()
    {
        await CriarOrdemAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", CriarPayloadOrdem());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("ordem de servico ativa");
    }

    [Fact]
    public async Task CancelarOrdem_DevePermitirNovaOsParaMesmoClienteEVeiculo()
    {
        var ordemCancelada = await CriarOrdemAsync();

        var cancelar = await _client.PatchAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCancelada.Id}/cancelar",
            new { motivoCancelamento = "Cliente desistiu do atendimento." });
        var novaOrdem = await CriarOrdemAsync();

        cancelar.StatusCode.Should().Be(HttpStatusCode.OK);
        novaOrdem.Id.Should().NotBe(ordemCancelada.Id);
        novaOrdem.Status.Should().Be(nameof(StatusOrdemDeServico.Recebida));
    }

    [Fact]
    public async Task CriarOrdem_DeveAceitarSnapshotsDeServicosEPecasNaAbertura()
    {
        var payload = new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Troca de pneus dianteiros.",
            observacoesRecepcao = "Cliente solicitou dois itens.",
            servicos = new[] { new { servicoId = CustomWebApplicationFactory.ServicoExistenteId } },
            pecas = new[] { new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 2 } }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.Servicos.Should().ContainSingle(item => item.ServicoId == CustomWebApplicationFactory.ServicoExistenteId);
        ordem.Pecas.Should().ContainSingle(item =>
            item.PecaId == CustomWebApplicationFactory.PecaExistenteId && item.Quantidade == 2);
        ordem.ValorTotal.Should().Be(240m);
    }

    [Fact]
    public async Task IniciarDiagnostico_DeveAceitarSolicitacaoAssincrona()
    {
        var ordem = await CriarOrdemAsync();

        var response = await _client.PatchAsync($"/api/v1/ordens-servico/{ordem.Id}/iniciar-diagnostico", null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task Entregar_DeveRetornarBadRequestQuandoOrdemNaoEstiverFinalizadaEPaga()
    {
        var ordem = await CriarOrdemAsync();

        var response = await _client.PatchAsync($"/api/v1/ordens-servico/{ordem.Id}/entregar", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MonitoramentoEEstimativa_DeveUsarDadosDaOs()
    {
        var ordem = await CriarOrdemAsync();

        var monitoramentoResponse = await _client.GetAsync($"/api/v1/ordens-servico/{ordem.Id}/monitoramento");
        var estimativaResponse = await _client.GetAsync($"/api/v1/ordens-servico/{ordem.Id}/estimativa-tempo-servico");
        var resumoResponse = await _client.GetAsync("/api/v1/ordens-servico/monitoramento?page=1&pageSize=10");

        monitoramentoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        estimativaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        resumoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var monitoramento = await monitoramentoResponse.Content.ReadFromJsonAsync<MonitoramentoOrdemDeServicoResponse>();
        monitoramento.Should().NotBeNull();
        monitoramento!.Status.Should().Be(nameof(StatusOrdemDeServico.Recebida));
        monitoramento.EstaFinalizada.Should().BeFalse();

        var estimativa = await estimativaResponse.Content.ReadFromJsonAsync<EstimativaTempoOrdemDeServicoResponse>();
        estimativa.Should().NotBeNull();
        estimativa!.TotalServicos.Should().Be(1);
        estimativa.TempoEstimadoMinutos.Should().Be(30);

        var resumo = await resumoResponse.Content.ReadFromJsonAsync<ResumoMonitoramentoOrdensDeServicoResponse>();
        resumo.Should().NotBeNull();
        resumo!.Ordens.Should().Contain(item => item.Id == ordem.Id);
    }

    [Fact]
    public async Task Listar_DeveOrdenarPorStatusEDataMaisAntigaEOcultarConcluidas()
    {
        SeedOrdensParaListagem();

        var response = await _client.GetAsync("/api/v1/ordens-servico?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<PagedResponse<OrdemDeServicoResponse>>();
        resultado.Should().NotBeNull();
        resultado!.Items.Select(item => item.Numero).Should().Equal(
            "OS-20260410-EM-EXECUCAO-ANTIGA",
            "OS-20260412-EM-EXECUCAO-NOVA",
            "OS-20260401-AGUARDANDO-APROVACAO",
            "OS-20260330-EM-DIAGNOSTICO",
            "OS-20260329-RECEBIDA");
        resultado.Items.Should().OnlyContain(item =>
            item.Status != nameof(StatusOrdemDeServico.Finalizada) &&
            item.Status != nameof(StatusOrdemDeServico.Entregue) &&
            item.Status != nameof(StatusOrdemDeServico.Cancelada));
    }

    [Theory]
    [InlineData("GET", "/api/v1/pecas")]
    [InlineData("POST", "/api/v1/pecas")]
    [InlineData("GET", "/api/v1/servicos")]
    [InlineData("POST", "/api/v1/servicos")]
    [InlineData("GET", "/api/v1/pedidos-compra")]
    [InlineData("POST", "/api/v1/pedidos-compra")]
    [InlineData("GET", "/api/v1/ordens-servico/3007/movimentacoes-estoque")]
    [InlineData("PATCH", "/api/v1/ordens-servico/3007/finalizar-diagnostico")]
    [InlineData("PATCH", "/api/v1/ordens-servico/3007/aprovar")]
    [InlineData("POST", "/api/v1/ordens-servico/3007/ordem/resposta")]
    [InlineData("PATCH", "/api/v1/ordens-servico/3007/liberar-execucao")]
    [InlineData("PATCH", "/api/v1/ordens-servico/3007/finalizar")]
    [InlineData("PATCH", "/api/v1/ordens-servico/OS-LEGADA/avancar-status")]
    [InlineData("POST", "/api/v1/ordens-servico/3007/servicos")]
    [InlineData("DELETE", "/api/v1/ordens-servico/3007/servicos/1000")]
    [InlineData("POST", "/api/v1/ordens-servico/3007/pecas")]
    [InlineData("DELETE", "/api/v1/ordens-servico/3007/pecas/1000")]
    [InlineData("PATCH", "/api/v1/ordens-servico/3007/registrar-pagamento")]
    public async Task RotasLegadas_DevemRetornarNotFound(string metodo, string rota)
    {
        using var request = new HttpRequestMessage(new HttpMethod(metodo), rota)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<OrdemDeServicoResponse> CriarOrdemAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", CriarPayloadOrdem());
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        return ordem!;
    }

    private static object CriarPayloadOrdem() => new
    {
        clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
        veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
        descricaoSolicitacao = "Cliente relatou barulho ao frear e puxando para a direita.",
        observacoesRecepcao = "Problema ocorre em baixa velocidade.",
        servicos = new[] { new { servicoId = CustomWebApplicationFactory.ServicoExistenteId } },
        pecas = Array.Empty<object>()
    };

    private void SeedOrdensParaListagem()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        context.OrdensDeServico.AddRange(
            CriarOrdemParaListagem("OS-20260412-EM-EXECUCAO-NOVA", "COD-LIST-001", StatusOrdemDeServico.EmExecucao, new DateTime(2026, 4, 12, 8, 0, 0)),
            CriarOrdemParaListagem("OS-20260410-EM-EXECUCAO-ANTIGA", "COD-LIST-002", StatusOrdemDeServico.EmExecucao, new DateTime(2026, 4, 10, 8, 0, 0)),
            CriarOrdemParaListagem("OS-20260401-AGUARDANDO-APROVACAO", "COD-LIST-003", StatusOrdemDeServico.AguardandoAprovacao, new DateTime(2026, 4, 1, 8, 0, 0)),
            CriarOrdemParaListagem("OS-20260330-EM-DIAGNOSTICO", "COD-LIST-004", StatusOrdemDeServico.EmDiagnostico, new DateTime(2026, 3, 30, 8, 0, 0)),
            CriarOrdemParaListagem("OS-20260329-RECEBIDA", "COD-LIST-005", StatusOrdemDeServico.Recebida, new DateTime(2026, 3, 29, 8, 0, 0)),
            CriarOrdemParaListagem("OS-20260328-FINALIZADA", "COD-LIST-006", StatusOrdemDeServico.Finalizada, new DateTime(2026, 3, 28, 8, 0, 0)),
            CriarOrdemParaListagem("OS-20260327-ENTREGUE", "COD-LIST-007", StatusOrdemDeServico.Entregue, new DateTime(2026, 3, 27, 8, 0, 0)),
            CriarOrdemParaListagem("OS-20260326-CANCELADA", "COD-LIST-008", StatusOrdemDeServico.Cancelada, new DateTime(2026, 3, 26, 8, 0, 0)));
        context.SaveChanges();
    }

    private static OrdemDeServico CriarOrdemParaListagem(
        string numero,
        string codigoAcompanhamento,
        StatusOrdemDeServico status,
        DateTime dataAbertura) =>
        OrdemDeServico.Restaurar(
            numero,
            codigoAcompanhamento,
            $"hash-{codigoAcompanhamento}",
            CustomWebApplicationFactory.PessoaFisicaClienteId,
            CustomWebApplicationFactory.VeiculoExistenteId,
            "Ordem criada para teste de listagem.",
            null,
            null,
            status,
            dataAbertura,
            status >= StatusOrdemDeServico.AguardandoAprovacao ? dataAbertura.AddHours(1) : null,
            status is StatusOrdemDeServico.Finalizada or StatusOrdemDeServico.Entregue ? dataAbertura.AddHours(2) : null,
            status == StatusOrdemDeServico.Entregue ? dataAbertura.AddHours(3) : null,
            status == StatusOrdemDeServico.Entregue ? dataAbertura.AddHours(4) : null,
            100m);
}
