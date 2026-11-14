using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reqnroll;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Bdd;

/// <summary>
/// Passos do fluxo da OS (F4-36). As acoes de pessoas (recepcao) vao pela API
/// HTTP do OS Service. Billing e Execucao entram pelos eventos que publicam,
/// entregues aos consumidores reais do OS Service (inbox, saga, historico e
/// outbox de verdade, num Postgres real). Os comandos que o OS Service manda
/// para os outros servicos sao conferidos no outbox.
/// </summary>
[Binding]
public sealed class FluxoOrdemDeServicoSteps(FluxoOrdemDeServicoContexto fluxo)
{
    private const int DiagnosticoId = 1;
    private static readonly JsonSerializerOptions Json = JsonSerializerOptions.Web;
    private OrcamentoAprovadoMessage? _ultimaAprovacao;

    private static BddWebApplicationFactory Api => BddHooks.Factory;

    private HttpClient Client => fluxo.Client ??= Api.CreateClient();

    // ---------- Pre-condicoes compostas ----------

    [Given("uma OS com orçamento de R$ {int} aguardando aprovação do cliente")]
    public async Task DadoOsAguardandoAprovacao(int valorTotal)
    {
        await DadoOsAberta(2);
        await QuandoRecepcaoSolicitaDiagnostico();
        await QuandoExecucaoIniciaDiagnostico();
        await QuandoExecucaoConcluiDiagnostico(150, 2, 45);
        await QuandoBillingGeraOrcamento();
        fluxo.ValorTotal.Should().Be(valorTotal);
        await EntaoOsFica("AguardandoAprovacao");
    }

    [Given("uma OS finalizada aguardando pagamento")]
    public async Task DadoOsFinalizadaAguardandoPagamento()
    {
        await DadoOsAguardandoAprovacao(240);
        await QuandoClienteAprovaOrcamento();
        await QuandoExecucaoReservaTodasAsPecas();
        await QuandoExecucaoFinalizaReparo();
        await EntaoSagaFica("AguardandoPagamento");
    }

    // ---------- Recepcao (API HTTP do OS Service) ----------

    [Given("uma OS aberta para o veículo do cliente com 1 serviço e {int} peças")]
    public async Task DadoOsAberta(int quantidadePecas)
    {
        var resposta = await Client.PostAsJsonAsync("/api/v1/ordens-servico", new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Barulho ao frear e carro puxando para a direita.",
            observacoesRecepcao = "Cliente aguarda orcamento por mensagem.",
            servicos = new[] { new { servicoId = CustomWebApplicationFactory.ServicoExistenteId } },
            pecas = new[] { new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = quantidadePecas } }
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Created, await resposta.Content.ReadAsStringAsync());
        var ordem = await resposta.Content.ReadFromJsonAsync<OrdemDeServicoResponse>(Json);
        fluxo.OrdemServicoId = ordem!.Id;
    }

    [When("a recepção solicita o diagnóstico")]
    public async Task QuandoRecepcaoSolicitaDiagnostico()
    {
        var resposta = await Client.PatchAsync($"/api/v1/ordens-servico/{fluxo.OrdemServicoId}/iniciar-diagnostico", null);
        resposta.StatusCode.Should().Be(HttpStatusCode.Accepted, await resposta.Content.ReadAsStringAsync());

        var saga = await ObterSagaAsync();
        fluxo.CorrelationId = saga.CorrelationId;
    }

    [When("a recepção entrega o veículo")]
    public async Task QuandoRecepcaoEntregaVeiculo() =>
        fluxo.UltimaResposta = await Client.PatchAsync($"/api/v1/ordens-servico/{fluxo.OrdemServicoId}/entregar", null);

    [When("a recepção pede uma nova cobrança")]
    public async Task QuandoRecepcaoPedeNovaCobranca() =>
        fluxo.UltimaResposta = await Client.PostAsync(
            $"/api/v1/ordens-servico/{fluxo.OrdemServicoId}/pagamento/nova-tentativa", null);

    // ---------- Execucao (eventos publicados pelo servico de Execucao) ----------

    [When("a Execução informa que o diagnóstico começou")]
    public Task QuandoExecucaoIniciaDiagnostico() =>
        EntregarAsync<DiagnosticoIniciadoConsumerWorker, DiagnosticoIniciadoMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrdemServicoId,
            DiagnosticoId, "EmAndamento", Agora()));

    [When("a Execução conclui o diagnóstico com 1 serviço de R$ {int} e {int} peças de R$ {int}")]
    public Task QuandoExecucaoConcluiDiagnostico(int precoServico, int quantidadePecas, int precoPeca)
    {
        fluxo.ValorServicos = precoServico;
        fluxo.ValorPecas = quantidadePecas * precoPeca;
        fluxo.QuantidadePecas = quantidadePecas;

        return EntregarAsync<DiagnosticoConcluidoConsumerWorker, DiagnosticoConcluidoMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrdemServicoId, DiagnosticoId,
            "Concluido",
            "Pastilhas no limite e geometria fora do padrao.",
            [new ServicoDiagnosticoConcluidoMessage(CustomWebApplicationFactory.ServicoExistenteId, "Alinhamento", precoServico, 30)],
            [new PecaDiagnosticoConcluidoMessage(CustomWebApplicationFactory.PecaExistenteId, "Pastilha de Freio", quantidadePecas, precoPeca)],
            Agora()));
    }

    [When("a Execução reserva todas as peças")]
    [When("a Execução repõe o estoque e reserva todas as peças")]
    public Task QuandoExecucaoReservaTodasAsPecas() =>
        EntregarAsync<PecasReservadasConsumerWorker, PecasReservadasMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrdemServicoId, fluxo.OrcamentoId,
            [new PecaReservaResultadoMessage(CustomWebApplicationFactory.PecaExistenteId,
                fluxo.QuantidadePecas, fluxo.QuantidadePecas, 0)]));

    [When("a Execução informa estoque insuficiente para {int} das {int} peças")]
    public Task QuandoExecucaoInformaEstoqueInsuficiente(int faltantes, int solicitadas) =>
        EntregarAsync<EstoqueInsuficienteConsumerWorker, EstoqueInsuficienteMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrdemServicoId, fluxo.OrcamentoId,
            [new PecaReservaResultadoMessage(CustomWebApplicationFactory.PecaExistenteId,
                solicitadas, solicitadas - faltantes, faltantes)]));

    [When("a Execução finaliza o reparo")]
    public Task QuandoExecucaoFinalizaReparo() =>
        EntregarAsync<ExecucaoFinalizadaConsumerWorker, ExecucaoFinalizadaMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrdemServicoId, DiagnosticoId,
            fluxo.OrcamentoId, "Finalizada", Agora()));

    // ---------- Billing (eventos publicados pelo servico de Billing) ----------

    [When("o Billing gera o orçamento")]
    public Task QuandoBillingGeraOrcamento()
    {
        fluxo.OrcamentoId = Guid.NewGuid();
        return EntregarAsync<OrcamentoGeradoConsumerWorker, OrcamentoGeradoMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrcamentoId, fluxo.OrdemServicoId,
            DiagnosticoId, "Gerado", fluxo.ValorServicos, fluxo.ValorPecas, fluxo.ValorTotal));
    }

    [Given("o cliente aprova o orçamento no Billing")]
    [When("o cliente aprova o orçamento no Billing")]
    public Task QuandoClienteAprovaOrcamento()
    {
        _ultimaAprovacao = new OrcamentoAprovadoMessage(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrcamentoId, fluxo.OrdemServicoId,
            DiagnosticoId, "Aprovado", fluxo.ValorTotal);
        return EntregarAsync<OrcamentoAprovadoConsumerWorker, OrcamentoAprovadoMessage>(_ultimaAprovacao);
    }

    [When("o Billing entrega a mesma aprovação do orçamento duas vezes")]
    public async Task QuandoBillingEntregaAprovacaoDuasVezes()
    {
        await QuandoClienteAprovaOrcamento();
        await EntregarAsync<OrcamentoAprovadoConsumerWorker, OrcamentoAprovadoMessage>(_ultimaAprovacao!);
    }

    [When("o cliente recusa o orçamento no Billing")]
    public Task QuandoClienteRecusaOrcamento() =>
        EntregarAsync<OrcamentoRecusadoConsumerWorker, OrcamentoRecusadoMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), fluxo.OrcamentoId, fluxo.OrdemServicoId,
            DiagnosticoId, "Recusado", "Cliente preferiu nao fazer o servico agora."));

    [When("o Billing confirma o pagamento")]
    public Task QuandoBillingConfirmaPagamento() =>
        EntregarAsync<PagamentoConfirmadoConsumerWorker, PagamentoConfirmadoMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), Guid.NewGuid(), fluxo.OrcamentoId,
            fluxo.OrdemServicoId, fluxo.ValorTotal, "Confirmado", Agora(),
            Tentativa: fluxo.TentativaPagamento));

    [When("o Billing recusa o pagamento com o motivo {string}")]
    public Task QuandoBillingRecusaPagamento(string motivo) =>
        EntregarAsync<PagamentoRecusadoConsumerWorker, PagamentoRecusadoMessage>(new(
            Guid.NewGuid(), fluxo.CorrelationId, Agora(), Guid.NewGuid(), fluxo.OrcamentoId,
            fluxo.OrdemServicoId, fluxo.ValorTotal, "Recusado", motivo, Agora(),
            Tentativa: fluxo.TentativaPagamento));

    // ---------- Verificacoes ----------

    [Then("a OS fica {string}")]
    public async Task EntaoOsFica(string status)
    {
        var ordem = await Client.GetFromJsonAsync<OrdemDeServicoResponse>(
            $"/api/v1/ordens-servico/{fluxo.OrdemServicoId}", Json);
        ordem!.Status.Should().Be(status);
    }

    [Then("a saga fica {string}")]
    public async Task EntaoSagaFica(string status)
    {
        var saga = await ObterSagaAsync();
        saga.Status.Should().Be(status);
    }

    [Then(@"^o OS Service envia o comando (\w+) para (?:a|o) (Execução|Billing)$")]
    public async Task EntaoOsServiceEnviaComando(string tipo, string servico)
    {
        var comandos = await ObterComandosAsync(tipo);
        comandos.Should().NotBeEmpty($"o OS Service deveria ter pedido {tipo} ao servico {servico}");
        comandos.Should().OnlyContain(item => item.Destino.StartsWith(Prefixo(servico)));
    }

    [Then(@"^o OS Service envia o comando (\w+) para (?:a|o) (Execução|Billing) uma única vez$")]
    public async Task EntaoOsServiceEnviaComandoUmaVez(string tipo, string servico)
    {
        var comandos = await ObterComandosAsync(tipo);
        comandos.Should().ContainSingle().Which.Destino.Should().StartWith(Prefixo(servico));
    }

    [Then(@"^o OS Service não envia o comando (\w+)$")]
    public async Task EntaoOsServiceNaoEnviaComando(string tipo) =>
        (await ObterComandosAsync(tipo)).Should().BeEmpty();

    [Then("o OS Service pede ao Billing a tentativa de cobrança {int}")]
    public async Task EntaoOsServicePedeTentativa(int tentativa)
    {
        var comandos = await ObterComandosAsync(nameof(SolicitarPagamentoMessage));
        var tentativas = comandos
            .Select(item => JsonSerializer.Deserialize<SolicitarPagamentoMessage>(item.Payload, Json)!.Tentativa)
            .ToArray();
        tentativas.Should().Equal(Enumerable.Range(1, tentativa));
        fluxo.TentativaPagamento = tentativa;
    }

    [Then("a entrega é recusada")]
    [Then("a nova cobrança é recusada")]
    public void EntaoRequisicaoRecusada() =>
        fluxo.UltimaResposta!.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    [Then("a nova cobrança é aceita")]
    public async Task EntaoNovaCobrancaAceita() =>
        fluxo.UltimaResposta!.StatusCode.Should().Be(
            HttpStatusCode.Accepted, await fluxo.UltimaResposta.Content.ReadAsStringAsync());

    [Then("o histórico da OS registra os eventos:")]
    public async Task EntaoHistoricoRegistra(DataTable eventos)
    {
        var historico = await Client.GetFromJsonAsync<List<OrdemServicoHistoricoResponse>>(
            $"/api/v1/ordens-servico/{fluxo.OrdemServicoId}/historico", Json);
        var registrados = historico!.Select(item => item.TipoEvento).ToArray();

        registrados.Should().Contain(eventos.Rows.Select(linha => linha["evento"]));
    }

    // ---------- Infraestrutura dos passos ----------

    /// <summary>Entrega o evento ao consumidor real, como o SQS faria.</summary>
    private static async Task EntregarAsync<TWorker, TMessage>(TMessage evento)
        where TWorker : SqsConsumerWorker<TMessage>
        where TMessage : IMensagemIntegracao
    {
        var consumidor = Api.Services.GetServices<IHostedService>().OfType<TWorker>().Single();
        await consumidor.ProcessarMensagemAsync(evento, CancellationToken.None);
    }

    private async Task<SagaInicioDiagnostico> ObterSagaAsync()
    {
        using var scope = Api.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        return await context.SagasInicioDiagnostico.AsNoTracking()
            .SingleAsync(item => item.OrdemServicoId == fluxo.OrdemServicoId);
    }

    private async Task<List<OutboxMessage>> ObterComandosAsync(string tipo)
    {
        using var scope = Api.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        return await context.OutboxMessages.AsNoTracking()
            .Where(item => item.CorrelationId == fluxo.CorrelationId && item.Tipo == tipo)
            .OrderBy(item => item.CriadoEm)
            .ToListAsync();
    }

    private static string Prefixo(string servico) => servico == "Billing" ? "Billing" : "Execucao";

    private static DateTimeOffset Agora() => DateTimeOffset.UtcNow;
}

/// <summary>Estado de um cenario, injetado pelo Reqnroll em cada classe de passos.</summary>
public sealed class FluxoOrdemDeServicoContexto
{
    public HttpClient? Client { get; set; }
    public int OrdemServicoId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid OrcamentoId { get; set; }
    public decimal ValorServicos { get; set; }
    public decimal ValorPecas { get; set; }
    public decimal ValorTotal => ValorServicos + ValorPecas;
    public int QuantidadePecas { get; set; }
    public int TentativaPagamento { get; set; } = 1;
    public HttpResponseMessage? UltimaResposta { get; set; }
}
