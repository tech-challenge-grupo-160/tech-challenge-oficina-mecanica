using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.OrdensDeServico;

/// <summary>
/// Compensacao de pagamento recusado (F4-27, ADR-0004): o pagamento e o passo
/// repetivel da saga. A execucao nao e desfeita, a entrega fica bloqueada e uma
/// nova cobranca pode ser pedida sem duplicar pagamento.
/// </summary>
public sealed class CompensacaoPagamentoTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 13, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Saga_RecusaDeveEsperarNovaTentativaSemDesfazerExecucao()
    {
        var saga = CriarSagaAguardandoPagamento();

        saga.MarcarPagamentoRecusado(1, Agora.AddMinutes(1)).Should().BeTrue();

        saga.Status.Should().Be("PagamentoRecusado");
        saga.TentativaPagamento.Should().Be(1);
        saga.AtualizadaEm.Should().Be(Agora.AddMinutes(1));
    }

    [Fact]
    public void Saga_RecusaRepetidaOuDeTentativaSuperadaDeveSerIgnorada()
    {
        var saga = CriarSagaAguardandoPagamento();
        saga.MarcarPagamentoRecusado(1, Agora);

        saga.MarcarPagamentoRecusado(1, Agora.AddMinutes(1)).Should().BeFalse();
        saga.IniciarNovaTentativaPagamento(Agora.AddMinutes(2));
        saga.MarcarPagamentoRecusado(1, Agora.AddMinutes(3)).Should().BeFalse();

        saga.Status.Should().Be("AguardandoPagamento");
        saga.TentativaPagamento.Should().Be(2);
    }

    [Fact]
    public void Saga_RecusaForaDeAguardandoPagamentoDeveFalhar()
    {
        var saga = SagaInicioDiagnostico.Criar(Guid.NewGuid(), 3000, Agora);
        saga.MarcarEmExecucao(Agora);

        var acao = () => saga.MarcarPagamentoRecusado(1, Agora);

        acao.Should().Throw<InvalidOperationException>().WithMessage("*EmExecucao*");
    }

    [Fact]
    public void Saga_NovaTentativaDeveVoltarAAguardarPagamentoESomarTentativa()
    {
        var saga = CriarSagaAguardandoPagamento();
        saga.MarcarPagamentoRecusado(1, Agora);

        saga.IniciarNovaTentativaPagamento(Agora.AddMinutes(1));
        saga.MarcarPagamentoRecusado(2, Agora.AddMinutes(2)).Should().BeTrue();
        saga.IniciarNovaTentativaPagamento(Agora.AddMinutes(3));
        saga.MarcarAguardandoEntrega(Agora.AddMinutes(4));

        saga.TentativaPagamento.Should().Be(3);
        saga.Status.Should().Be("AguardandoEntrega");
    }

    [Fact]
    public void Saga_NovaTentativaSemRecusaDeveFalhar()
    {
        var saga = CriarSagaAguardandoPagamento();

        var acao = () => saga.IniciarNovaTentativaPagamento(Agora);

        acao.Should().Throw<InvalidOperationException>().WithMessage("*AguardandoPagamento*");
        saga.TentativaPagamento.Should().Be(1);
    }

    [Fact]
    public void Ordem_RecusaDeveManterFinalizadaEBloquearEntrega()
    {
        var ordem = CriarOrdemFinalizada();

        var evento = ordem.RegistrarPagamentoRecusado("cancelled: expired", 1);

        ordem.Status.Should().Be(StatusOrdemDeServico.Finalizada);
        ordem.DataPagamento.Should().BeNull();
        evento.TipoEvento.Should().Be(TipoEventoOrdemServico.PagamentoRecusado);
        evento.StatusAnterior.Should().Be(StatusOrdemDeServico.Finalizada);
        evento.StatusNovo.Should().Be(StatusOrdemDeServico.Finalizada);
        evento.Descricao.Should().Contain("tentativa 1").And.Contain("cancelled: expired");
        var entrega = () => ordem.Entregar(DateTime.UtcNow);
        entrega.Should().Throw<InvalidOperationException>().WithMessage("*pagamento*");
    }

    [Fact]
    public void Ordem_RecusaExigeOrdemFinalizadaSemPagamentoEMotivo()
    {
        var emExecucao = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmExecucao);
        var paga = CriarOrdemFinalizada();
        paga.RegistrarPagamento(DateTime.UtcNow);

        ((Action)(() => emExecucao.RegistrarPagamentoRecusado("rejected", 1)))
            .Should().Throw<InvalidOperationException>().WithMessage("*finalizada*");
        ((Action)(() => paga.RegistrarPagamentoRecusado("rejected", 1)))
            .Should().Throw<InvalidOperationException>().WithMessage("*pagamento ja foi registrado*");
        ((Action)(() => CriarOrdemFinalizada().RegistrarPagamentoRecusado(" ", 1)))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ordem_NovaCobrancaComecaNaSegundaTentativa()
    {
        var ordem = CriarOrdemFinalizada();

        var evento = ordem.SolicitarNovaCobranca(2);

        evento.TipoEvento.Should().Be(TipoEventoOrdemServico.NovaCobrancaSolicitada);
        evento.Descricao.Should().Contain("tentativa 2");
        ((Action)(() => ordem.SolicitarNovaCobranca(1))).Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task ConfirmarRecusa_DeveRegistrarHistoricoSemMudarStatus()
    {
        var ordem = CriarOrdemFinalizada();
        var historico = new List<OrdemServicoHistorico>();
        var handler = new ConfirmarPagamentoRecusadoCommandHandler(
            CriarRepository(ordem).Object, CriarHistoricoService(historico));

        await handler.Handle(
            new ConfirmarPagamentoRecusadoCommand(ordem.Id, "rejected: cc_rejected_insufficient_amount", 1),
            CancellationToken.None);

        ordem.Status.Should().Be(StatusOrdemDeServico.Finalizada);
        historico.Should().ContainSingle(item =>
            item.TipoEvento == TipoEventoOrdemServico.PagamentoRecusado &&
            item.StatusNovo == StatusOrdemDeServico.Finalizada);
    }

    [Fact]
    public async Task ConfirmarRecusa_DeveFalharQuandoOrdemNaoExistir()
    {
        var handler = new ConfirmarPagamentoRecusadoCommandHandler(
            Mock.Of<IOrdemDeServicoRepository>(), CriarHistoricoService([]));

        var acao = () => handler.Handle(
            new ConfirmarPagamentoRecusadoCommand(9999, "rejected", 1), CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>();
    }

    [Fact]
    public async Task NovaCobranca_DeveAbrirProximaTentativaERegistrarHistorico()
    {
        var ordem = CriarOrdemFinalizada();
        var historico = new List<OrdemServicoHistorico>();
        var saga = new Mock<ISagaPagamentoService>();
        saga.Setup(item => item.SolicitarNovaTentativaAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        var handler = new SolicitarNovaCobrancaCommandHandler(
            CriarRepository(ordem).Object, saga.Object, CriarHistoricoService(historico));

        var result = await handler.Handle(new SolicitarNovaCobrancaCommand { Id = ordem.Id }, CancellationToken.None);

        result.Id.Should().Be(ordem.Id);
        result.Status.Should().Be(nameof(StatusOrdemDeServico.Finalizada));
        saga.Verify(item => item.SolicitarNovaTentativaAsync(ordem.Id, It.IsAny<CancellationToken>()), Times.Once);
        historico.Should().ContainSingle(item =>
            item.TipoEvento == TipoEventoOrdemServico.NovaCobrancaSolicitada &&
            item.Descricao.Contains("tentativa 2"));
    }

    [Fact]
    public async Task NovaCobranca_NaoDeveTocarNaSagaSeAOrdemJaFoiPaga()
    {
        var ordem = CriarOrdemFinalizada();
        ordem.RegistrarPagamento(DateTime.UtcNow);
        var saga = new Mock<ISagaPagamentoService>();
        var handler = new SolicitarNovaCobrancaCommandHandler(
            CriarRepository(ordem).Object, saga.Object, CriarHistoricoService([]));

        var acao = () => handler.Handle(new SolicitarNovaCobrancaCommand { Id = ordem.Id }, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
        saga.Verify(item => item.SolicitarNovaTentativaAsync(
            It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NovaCobranca_DeveFalharQuandoOrdemNaoExistir()
    {
        var handler = new SolicitarNovaCobrancaCommandHandler(
            Mock.Of<IOrdemDeServicoRepository>(), Mock.Of<ISagaPagamentoService>(), CriarHistoricoService([]));

        var acao = () => handler.Handle(new SolicitarNovaCobrancaCommand { Id = 9999 }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>();
    }

    [Fact]
    public void Validator_DeveRejeitarIdInvalido()
    {
        var validator = new SolicitarNovaCobrancaCommandValidator();

        validator.Validate(new SolicitarNovaCobrancaCommand { Id = 0 }).IsValid.Should().BeFalse();
        validator.Validate(new SolicitarNovaCobrancaCommand { Id = 3000 }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Contratos_DevemCarregarTentativaComPadraoUm()
    {
        var json = JsonSerializerOptions.Web;
        var recusado = new PagamentoRecusadoMessage(
            Guid.NewGuid(), Guid.NewGuid(), Agora, Guid.NewGuid(), Guid.NewGuid(), 3000,
            219.90m, "Recusado", "cancelled: expired", Agora, Tentativa: 2);
        var solicitarAntigo = JsonSerializer.Deserialize<SolicitarPagamentoMessage>(
            """{"messageId":"0b8f2a3e-5d1c-4e8b-9a8f-1c2d3e4f5a6b","correlationId":"1b8f2a3e-5d1c-4e8b-9a8f-1c2d3e4f5a6b","ocorridoEm":"2026-11-13T20:00:00+00:00","orcamentoId":"2b8f2a3e-5d1c-4e8b-9a8f-1c2d3e4f5a6b","ordemServicoId":3000,"versao":1}""",
            json);
        var recusadoLido = JsonSerializer.Deserialize<PagamentoRecusadoMessage>(
            JsonSerializer.Serialize(recusado, json), json);
        var options = new MessagingOptions { PagamentoRecusadoQueueUrl = "http://localhost:4566/os-pagamento-recusado" };

        solicitarAntigo!.Tentativa.Should().Be(1);
        recusadoLido.Should().Be(recusado);
        recusadoLido!.Tentativa.Should().Be(2);
        options.PagamentoRecusadoQueueUrl.Should().EndWith("os-pagamento-recusado");
    }

    private static SagaInicioDiagnostico CriarSagaAguardandoPagamento()
    {
        var saga = SagaInicioDiagnostico.Criar(Guid.NewGuid(), 3000, Agora);
        saga.Concluir(1, Agora);
        saga.ConfirmarOrcamento(Guid.NewGuid(), Agora);
        saga.MarcarEmExecucao(Agora);
        saga.MarcarAguardandoPagamento(Agora);
        return saga;
    }

    private static OrdemDeServico CriarOrdemFinalizada()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmExecucao);
        ordem.FinalizarServico(new DateTime(2026, 11, 13, 17, 0, 0));
        return ordem;
    }

    private static Mock<IOrdemDeServicoRepository> CriarRepository(OrdemDeServico ordem)
    {
        var repository = new Mock<IOrdemDeServicoRepository>();
        repository.Setup(item => item.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        repository.Setup(item => item.AtualizarAsync(ordem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        return repository;
    }

    private static OrdemDeServicoHistoricoService CriarHistoricoService(List<OrdemServicoHistorico> registros)
    {
        var historico = new Mock<IOrdemServicoHistoricoRepository>();
        historico.Setup(item => item.CriarAsync(
                It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServicoHistorico, CancellationToken>((item, _) => registros.Add(item))
            .ReturnsAsync((OrdemServicoHistorico item, CancellationToken _) => item);
        var usuario = new Mock<IUsuarioAutenticadoService>();
        usuario.Setup(item => item.ObterUsuarioAtual()).Returns(new UsuarioAutenticadoInfo());
        var clock = new Mock<IClock>();
        clock.SetupGet(item => item.Now).Returns(DateTime.UtcNow);

        return new OrdemDeServicoHistoricoService(
            historico.Object,
            usuario.Object,
            clock.Object,
            Mock.Of<IBusinessMetrics>());
    }
}
