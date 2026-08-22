using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.PedidosCompra;
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
    public async Task FluxoCompleto_DevePercorrerCaminhoFelizDaOs()
    {
        var ordemCriada = await CriarOrdemAsync();

        var iniciarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        iniciarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);

        var adicionarPeca = await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/pecas",
            new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 1 });
        adicionarPeca.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalizarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);
        finalizarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);
        var aguardandoAprovacao = await finalizarDiagnostico.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        aguardandoAprovacao.Should().NotBeNull();
        aguardandoAprovacao!.Status.Should().Be("AguardandoAprovacao");
        aguardandoAprovacao.OrcamentoEnviadoEm.Should().NotBeNull();
        aguardandoAprovacao.ValorTotal.Should().Be(195m);

        var aprovar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);
        aprovar.StatusCode.Should().Be(HttpStatusCode.OK);
        var emExecucao = await aprovar.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        emExecucao!.Status.Should().Be("EmExecucao");

        var finalizar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar", null);
        finalizar.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalizada = await finalizar.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        finalizada!.Status.Should().Be("Finalizada");
        finalizada.DataFinalizacao.Should().NotBeNull();

        var registrarPagamento = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/registrar-pagamento", null);
        registrarPagamento.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagamentoRegistrado = await registrarPagamento.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        pagamentoRegistrado!.Status.Should().Be("Finalizada");
        pagamentoRegistrado.DataPagamento.Should().NotBeNull();

        var entregar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/entregar", null);
        entregar.StatusCode.Should().Be(HttpStatusCode.OK);
        var entregue = await entregar.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        entregue!.Status.Should().Be("Entregue");
        entregue.DataConclusao.Should().NotBeNull();

        var obterHistorico = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/historico");
        obterHistorico.StatusCode.Should().Be(HttpStatusCode.OK);
        var historico = await obterHistorico.Content.ReadFromJsonAsync<List<OrdemServicoHistoricoResponse>>();
        historico.Should().NotBeNull();
        historico.Should().HaveCount(10);
        historico![0].TipoEvento.Should().Be("OrdemCriada");
        historico[0].StatusAnterior.Should().BeNull();
        historico[0].StatusNovo.Should().Be("Recebida");
        historico.Should().OnlyContain(h =>
            h.UsuarioId == CustomWebApplicationFactory.UsuarioAutenticadoId &&
            h.UsuarioNome == CustomWebApplicationFactory.UsuarioAutenticadoNome);
        historico.Should().Contain(h => h.TipoEvento == "EstoqueBaixado");
        historico[^1].TipoEvento.Should().Be("VeiculoEntregue");
        historico[^1].StatusAnterior.Should().Be("Finalizada");
        historico[^1].StatusNovo.Should().Be("Entregue");

        var obterNotificacoes = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/notificacoes");
        obterNotificacoes.StatusCode.Should().Be(HttpStatusCode.OK);
        var notificacoes = await obterNotificacoes.Content.ReadFromJsonAsync<List<NotificacaoClienteResponse>>();
        notificacoes.Should().NotBeNull();
        notificacoes.Should().HaveCount(3);
        notificacoes.Should().Contain(n => n.TipoNotificacao == "LinkAcompanhamentoEnviado" && n.Canal == "Email" && n.Recebida);
        notificacoes.Should().Contain(n => n.TipoNotificacao == "OrcamentoDisponivel" && n.Canal == "WhatsApp" && n.Recebida);
        notificacoes.Should().Contain(n => n.TipoNotificacao == "ServicoFinalizado" && n.Canal == "WhatsApp" && n.Recebida);
    }

    [Fact]
    public async Task FinalizarDiagnostico_DeveRetornarBadRequestQuandoNaoHouverServico()
    {
        var ordemCriada = await CriarOrdemAsync();

        var iniciarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        iniciarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);

        var removerServico = await _client.DeleteAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/servicos/{CustomWebApplicationFactory.ServicoExistenteId}");
        removerServico.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalizarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        finalizarDiagnostico.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await finalizarDiagnostico.Content.ReadAsStringAsync();
        body.Should().Contain("ao menos um servico");
    }

    [Fact]
    public async Task CriarOrdem_DeveRetornarBadRequestQuandoJaExistirOsAtivaParaMesmoClienteEVeiculo()
    {
        var primeiraOrdem = await CriarOrdemAsync();

        var payload = new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Nova tentativa para o mesmo cliente e veiculo.",
            observacoesRecepcao = "Nao deve permitir duplicidade.",
            servicos = new[]
            {
                new { servicoId = CustomWebApplicationFactory.ServicoExistenteId }
            },
            pecas = Array.Empty<object>()
        };

        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ordem de servico ativa");
    }

    [Fact]
    public async Task CriarOrdem_DevePermitirNovaOsParaMesmoClienteEVeiculoQuandoAnteriorEstiverCancelada()
    {
        var ordemCancelada = await CriarOrdemAsync();

        var cancelar = await _client.PatchAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCancelada.Id}/cancelar",
            new { motivoCancelamento = "Cliente desistiu do atendimento." });
        cancelar.StatusCode.Should().Be(HttpStatusCode.OK);

        var novaOrdem = await CriarOrdemAsync();

        novaOrdem.Id.Should().NotBe(ordemCancelada.Id);
        novaOrdem.Status.Should().Be(nameof(StatusOrdemDeServico.Recebida));
        novaOrdem.ClienteId.Should().Be(ordemCancelada.ClienteId);
        novaOrdem.VeiculoId.Should().Be(ordemCancelada.VeiculoId);
    }

    [Fact]
    public async Task CriarOrdem_DeveRetornarBadRequestQuandoAnteriorEstiverFinalizada()
    {
        var ordemFinalizada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemFinalizada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemFinalizada.Id}/finalizar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemFinalizada.Id}/aprovar", null);
        var finalizar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemFinalizada.Id}/finalizar", null);
        finalizar.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", CriarPayloadOrdem());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ordem de servico ativa");
    }

    [Fact]
    public async Task CriarOrdem_DevePermitirNovaOsParaMesmoClienteEVeiculoQuandoAnteriorEstiverEntregue()
    {
        var ordemEntregue = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemEntregue.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemEntregue.Id}/finalizar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemEntregue.Id}/aprovar", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemEntregue.Id}/finalizar", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemEntregue.Id}/registrar-pagamento", null);
        var entregar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemEntregue.Id}/entregar", null);
        entregar.StatusCode.Should().Be(HttpStatusCode.OK);

        var novaOrdem = await CriarOrdemAsync();

        novaOrdem.Id.Should().NotBe(ordemEntregue.Id);
        novaOrdem.Status.Should().Be(nameof(StatusOrdemDeServico.Recebida));
        novaOrdem.ClienteId.Should().Be(ordemEntregue.ClienteId);
        novaOrdem.VeiculoId.Should().Be(ordemEntregue.VeiculoId);
    }

    [Fact]
    public async Task CriarOrdem_DevePermitirInformarServicosEPecasNaAbertura()
    {
        var payload = new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Troca de pneus dianteiros.",
            observacoesRecepcao = "Cliente solicitou dois pneus da frente.",
            servicos = new[]
            {
                new { servicoId = CustomWebApplicationFactory.ServicoExistenteId }
            },
            pecas = new[]
            {
                new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 2 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.Status.Should().Be(nameof(StatusOrdemDeServico.Recebida));
        ordem.Servicos.Should().ContainSingle(x => x.ServicoId == CustomWebApplicationFactory.ServicoExistenteId);
        ordem.Pecas.Should().ContainSingle(x =>
            x.PecaId == CustomWebApplicationFactory.PecaExistenteId &&
            x.Quantidade == 2);
        ordem.ValorTotal.Should().Be(240m);
    }

    [Fact]
    public async Task Entregar_DeveRetornarBadRequestQuandoPagamentoNaoTiverSidoRegistrado()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar", null);

        var entregar = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/entregar", null);

        entregar.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await entregar.Content.ReadAsStringAsync();
        body.Should().Contain("pagamento");
    }

    [Fact]
    public async Task ObterMonitoramento_DeveRetornarTempoDeFinalizacaoDaOs()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar", null);

        var response = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/monitoramento");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var monitoramento = await response.Content.ReadFromJsonAsync<MonitoramentoOrdemDeServicoResponse>();
        monitoramento.Should().NotBeNull();
        monitoramento!.Id.Should().Be(ordemCriada.Id);
        monitoramento.EstaFinalizada.Should().BeTrue();
        monitoramento.DataFinalizacao.Should().NotBeNull();
        monitoramento.TempoFinalizacaoMinutos.Should().NotBeNull();
        monitoramento.TempoFinalizacaoMinutos.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ObterEstimativaTempo_DeveRetornarTempoEstimadoComBaseNosServicosDaOs()
    {
        var ordemCriada = await CriarOrdemAsync();

        var response = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/estimativa-tempo-servico");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var estimativa = await response.Content.ReadFromJsonAsync<EstimativaTempoOrdemDeServicoResponse>();
        estimativa.Should().NotBeNull();
        estimativa!.OrdemDeServicoId.Should().Be(ordemCriada.Id);
        estimativa.TotalServicos.Should().Be(1);
        estimativa.TempoEstimadoMinutos.Should().Be(30);
        estimativa.TempoEstimadoHoras.Should().Be(0.5);
        estimativa.Servicos.Should().ContainSingle(x =>
            x.ServicoId == CustomWebApplicationFactory.ServicoExistenteId &&
            x.TempoEstimadoMinutos == 30 &&
            x.TempoEstimadoHoras == 0.5);
    }

    [Fact]
    public async Task ObterResumoMonitoramento_DeveRetornarVisaoGeralComMedia()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar", null);

        var response = await _client.GetAsync("/api/v1/ordens-servico/monitoramento?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resumo = await response.Content.ReadFromJsonAsync<ResumoMonitoramentoOrdensDeServicoResponse>();
        resumo.Should().NotBeNull();
        resumo!.TotalOrdens.Should().BeGreaterThanOrEqualTo(1);
        resumo.TotalOrdensFinalizadas.Should().BeGreaterThanOrEqualTo(1);
        resumo.Page.Should().Be(1);
        resumo.PageSize.Should().Be(2);
        resumo.TotalPages.Should().BeGreaterThanOrEqualTo(1);
        resumo.Ordens.Should().HaveCountLessThanOrEqualTo(2);
        resumo.TempoMedioFinalizacaoMinutos.Should().NotBeNull();
        resumo.Ordens.Should().Contain(x => x.Id == ordemCriada.Id && x.EstaFinalizada);
    }

    [Fact]
    public async Task LiberarExecucao_DeveValidarEstoqueBaixarItensEAtualizarStatusAposRecebimento()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/pecas",
            new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 101 });
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        var primeiraAprovacao = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);

        primeiraAprovacao.StatusCode.Should().Be(HttpStatusCode.OK);
        var aguardandoEstoque = await primeiraAprovacao.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        aguardandoEstoque.Should().NotBeNull();
        aguardandoEstoque!.Status.Should().Be("AguardandoEstoque");

        var pedidosResponse = await _client.GetAsync($"/api/v1/pedidos-compra/ordem/{ordemCriada.Id}");

        pedidosResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var pedidos = await pedidosResponse.Content.ReadFromJsonAsync<List<PedidoCompraResponse>>();
        pedidos.Should().NotBeNull();
        pedidos.Should().ContainSingle();
        pedidos![0].QuantidadeSolicitada.Should().Be(1);
        pedidos[0].Status.Should().Be("Pendente");

        var receberPedido = await _client.PatchAsJsonAsync(
            $"/api/v1/pedidos-compra/{pedidos[0].Id}/receber",
            new { quantidadeRecebida = 1 });

        receberPedido.StatusCode.Should().Be(HttpStatusCode.OK);
        var pedidoRecebido = await receberPedido.Content.ReadFromJsonAsync<PedidoCompraResponse>();
        pedidoRecebido.Should().NotBeNull();
        pedidoRecebido!.Status.Should().Be("Recebido");
        pedidoRecebido.QuantidadeRecebida.Should().Be(1);

        var aprovacaoIndevida = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/aprovar", null);

        aprovacaoIndevida.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var mensagemErroAprovacao = await aprovacaoIndevida.Content.ReadAsStringAsync();
        mensagemErroAprovacao.Should().Contain("aguardando estoque");

        var liberacaoExecucao = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/liberar-execucao", null);

        liberacaoExecucao.StatusCode.Should().Be(HttpStatusCode.OK);
        var emExecucao = await liberacaoExecucao.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        emExecucao.Should().NotBeNull();
        emExecucao!.Status.Should().Be("EmExecucao");

        var movimentacoesResponse = await _client.GetAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/movimentacoes-estoque");

        movimentacoesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var movimentacoesPorPeca = await movimentacoesResponse.Content.ReadFromJsonAsync<List<MovimentacoesEstoquePorPecaResponse>>();
        movimentacoesPorPeca.Should().NotBeNull();
        movimentacoesPorPeca.Should().ContainSingle(x => x.PecaId == CustomWebApplicationFactory.PecaExistenteId);

        var grupoPeca = movimentacoesPorPeca!.Single(x => x.PecaId == CustomWebApplicationFactory.PecaExistenteId);
        grupoPeca.QuantidadeNaOrdem.Should().Be(101);
        grupoPeca.TotalMovimentacoes.Should().Be(2);
        grupoPeca.Movimentacoes.Should().HaveCount(2);
        grupoPeca.Movimentacoes.Should().Contain(x =>
            x.TipoMovimentacao == "EntradaPorPedidoCompra" &&
            x.Quantidade == 1 &&
            x.QuantidadePosterior == 101);
        grupoPeca.Movimentacoes.Should().Contain(x =>
            x.TipoMovimentacao == "BaixaParaOrdemDeServico" &&
            x.Quantidade == 101 &&
            x.QuantidadePosterior == 0);
    }

    [Fact]
    public async Task ResponderOrdem_DeveAprovarOsComTokenDeAcompanhamento()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ordens-servico/{ordemCriada.Id}/ordem/resposta")
        {
            Content = JsonContent.Create(new { aprovado = true })
        };
        request.Headers.Add(TestAuthHandler.TestRoleHeaderName, "Cliente");
        request.Headers.Add(TestAuthHandler.TestDocumentoHeaderName, "47654866801");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.Status.Should().Be(nameof(StatusOrdemDeServico.EmExecucao));
    }

    [Fact]
    public async Task ResponderOrdem_DeveRecusarOsComTokenDeAcompanhamento()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ordens-servico/{ordemCriada.Id}/ordem/resposta")
        {
            Content = JsonContent.Create(new
            {
                aprovado = false,
                motivoRecusa = "Cliente recusou a ordem de servico."
            })
        };
        request.Headers.Add(TestAuthHandler.TestRoleHeaderName, "Cliente");
        request.Headers.Add(TestAuthHandler.TestDocumentoHeaderName, "47654866801");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.Status.Should().Be(nameof(StatusOrdemDeServico.Cancelada));
        ordem.MotivoCancelamento.Should().Be("Cliente recusou a ordem de servico.");
    }

    [Fact]
    public async Task ResponderOrdem_DeveRetornarForbiddenQuandoTokenNaoForDeCliente()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ordens-servico/{ordemCriada.Id}/ordem/resposta")
        {
            Content = JsonContent.Create(new { aprovado = true })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ResponderOrdem_DeveRetornarNotFoundQuandoDocumentoNaoPertencerAoClienteDaOrdem()
    {
        var ordemCriada = await CriarOrdemAsync();

        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ordens-servico/{ordemCriada.Id}/ordem/resposta")
        {
            Content = JsonContent.Create(new { aprovado = true })
        };
        request.Headers.Add(TestAuthHandler.TestRoleHeaderName, "Cliente");
        request.Headers.Add(TestAuthHandler.TestDocumentoHeaderName, "60617051000199");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Listar_DeveOrdenarPorStatusEDataMaisAntigaEOcultarFinalizadasEntreguesECanceladas()
    {
        SeedOrdensParaListagem();

        var response = await _client.GetAsync("/api/v1/ordens-servico?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<PagedResponse<OrdemDeServicoResponse>>();
        resultado.Should().NotBeNull();
        resultado!.TotalItems.Should().Be(5);
        resultado.Items.Select(x => x.Numero).Should().Equal(
            "OS-20260410-EM-EXECUCAO-ANTIGA",
            "OS-20260412-EM-EXECUCAO-NOVA",
            "OS-20260401-AGUARDANDO-APROVACAO",
            "OS-20260330-EM-DIAGNOSTICO",
            "OS-20260329-RECEBIDA");
        resultado.Items.Should().OnlyContain(x =>
            x.Status != nameof(StatusOrdemDeServico.Finalizada) &&
            x.Status != nameof(StatusOrdemDeServico.Entregue) &&
            x.Status != nameof(StatusOrdemDeServico.Cancelada));
    }

    [Fact]
    public async Task RemoverItens_DevePermitirSomenteQuandoOsEstiverEmDiagnostico()
    {
        var ordemCriada = await CriarOrdemAsync();

        var iniciarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        iniciarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);

        await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/pecas",
            new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 1 });

        var removerServico = await _client.DeleteAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/servicos/{CustomWebApplicationFactory.ServicoExistenteId}");
        removerServico.StatusCode.Should().Be(HttpStatusCode.OK);
        var ordemSemServico = await removerServico.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordemSemServico.Should().NotBeNull();
        ordemSemServico!.Servicos.Should().BeEmpty();

        await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/servicos",
            new { servicoId = CustomWebApplicationFactory.ServicoExistenteId });
        await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/finalizar-diagnostico", null);

        var removerPeca = await _client.DeleteAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/pecas/{CustomWebApplicationFactory.PecaExistenteId}");
        removerPeca.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await removerPeca.Content.ReadAsStringAsync();
        body.Should().Contain("remover pecas durante o diagnostico");
    }

    [Fact]
    public async Task AdicionarServico_DeveRetornarBadRequestQuandoServicoJaExistirNaOs()
    {
        var ordemCriada = await CriarOrdemAsync();

        var iniciarDiagnostico = await _client.PatchAsync($"/api/v1/ordens-servico/{ordemCriada.Id}/iniciar-diagnostico", null);
        iniciarDiagnostico.StatusCode.Should().Be(HttpStatusCode.OK);

        var segundaInclusao = await _client.PostAsJsonAsync(
            $"/api/v1/ordens-servico/{ordemCriada.Id}/servicos",
            new { servicoId = CustomWebApplicationFactory.ServicoExistenteId });

        segundaInclusao.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await segundaInclusao.Content.ReadAsStringAsync();
        body.Should().Contain("ja foi adicionado");
    }

    private async Task<OrdemDeServicoResponse> CriarOrdemAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/ordens-servico", CriarPayloadOrdem());
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.Servicos.Should().ContainSingle(x => x.ServicoId == CustomWebApplicationFactory.ServicoExistenteId);
        ordem.Pecas.Should().BeEmpty();
        return ordem;
    }

    private static object CriarPayloadOrdem()
    {
        return new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Cliente relatou barulho ao frear e puxando para a direita.",
            observacoesRecepcao = "Problema ocorre em baixa velocidade.",
            servicos = new[]
            {
                new { servicoId = CustomWebApplicationFactory.ServicoExistenteId }
            },
            pecas = Array.Empty<object>()
        };
    }

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
        DateTime dataAbertura)
    {
        return OrdemDeServico.Restaurar(
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
            status == StatusOrdemDeServico.Finalizada || status == StatusOrdemDeServico.Entregue ? dataAbertura.AddHours(2) : null,
            status == StatusOrdemDeServico.Entregue ? dataAbertura.AddHours(3) : null,
            status == StatusOrdemDeServico.Entregue ? dataAbertura.AddHours(4) : null,
            100m);
    }
}
