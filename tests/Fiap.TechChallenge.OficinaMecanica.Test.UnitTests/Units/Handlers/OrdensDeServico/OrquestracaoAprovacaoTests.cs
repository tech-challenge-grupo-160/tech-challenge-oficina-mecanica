using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.OrdensDeServico;

public sealed class OrquestracaoAprovacaoTests
{
    [Fact]
    public async Task AprovarSemPecas_DeveLiberarExecucaoSemSolicitarReserva()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoAprovacao);
        var repository = CriarRepository(ordem);
        var handler = new ConfirmarOrcamentoAprovadoCommandHandler(
            repository.Object, CriarHistoricoService());

        var result = await handler.Handle(
            new ConfirmarOrcamentoAprovadoCommand(ordem.Id, ordem.ValorTotal),
            CancellationToken.None);

        result.ReservaNecessaria.Should().BeFalse();
        ordem.Status.Should().Be(StatusOrdemDeServico.EmExecucao);
    }

    [Fact]
    public async Task AprovarComPecas_DeveManterStatusAteRespostaDaExecucao()
    {
        var ordem = CriarAguardandoAprovacaoComPeca();
        var repository = CriarRepository(ordem);
        var handler = new ConfirmarOrcamentoAprovadoCommandHandler(
            repository.Object, CriarHistoricoService());

        var result = await handler.Handle(
            new ConfirmarOrcamentoAprovadoCommand(ordem.Id, ordem.ValorTotal),
            CancellationToken.None);

        result.ReservaNecessaria.Should().BeTrue();
        result.Pecas.Should().ContainSingle(item => item.PecaId == 1000 && item.Quantidade == 4);
        ordem.Status.Should().Be(StatusOrdemDeServico.AguardandoAprovacao);
    }

    [Fact]
    public async Task EstoqueInsuficiente_DeveColocarOrdemEmAguardandoEstoque()
    {
        var ordem = CriarAguardandoAprovacaoComPeca();
        var handler = new ConfirmarReservaPecasCommandHandler(
            CriarRepository(ordem).Object, CriarHistoricoService());

        await handler.Handle(new ConfirmarReservaPecasCommand(
            ordem.Id, false, "peca 1000: faltam 2"), CancellationToken.None);

        ordem.Status.Should().Be(StatusOrdemDeServico.AguardandoEstoque);
    }

    [Fact]
    public async Task ReposicaoConcluida_DeveLiberarOrdemQueAguardavaEstoque()
    {
        var ordem = CriarAguardandoAprovacaoComPeca();
        var handler = new ConfirmarReservaPecasCommandHandler(
            CriarRepository(ordem).Object, CriarHistoricoService());
        await handler.Handle(new ConfirmarReservaPecasCommand(
            ordem.Id, false, "peca 1000: faltam 2"), CancellationToken.None);

        await handler.Handle(new ConfirmarReservaPecasCommand(
            ordem.Id, true, null), CancellationToken.None);

        ordem.Status.Should().Be(StatusOrdemDeServico.EmExecucao);
    }

    [Fact]
    public async Task EstoqueInsuficienteRepetido_DeveSerIdempotenteEnquantoAguardaReposicao()
    {
        var ordem = CriarAguardandoAprovacaoComPeca();
        var handler = new ConfirmarReservaPecasCommandHandler(
            CriarRepository(ordem).Object, CriarHistoricoService());
        await handler.Handle(new ConfirmarReservaPecasCommand(
            ordem.Id, false, "peca 1000: faltam 2"), CancellationToken.None);

        await handler.Handle(new ConfirmarReservaPecasCommand(
            ordem.Id, false, "peca 1000: faltam 2"), CancellationToken.None);

        ordem.Status.Should().Be(StatusOrdemDeServico.AguardandoEstoque);
    }

    [Fact]
    public async Task Recusar_DeveCancelarOrdemComMotivoDoCliente()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoAprovacao);
        var handler = new ConfirmarOrcamentoRecusadoCommandHandler(
            CriarRepository(ordem).Object, CriarHistoricoService());

        await handler.Handle(new ConfirmarOrcamentoRecusadoCommand(
            ordem.Id, "Valor nao aprovado."), CancellationToken.None);

        ordem.Status.Should().Be(StatusOrdemDeServico.Cancelada);
        ordem.MotivoCancelamento.Should().Be("Valor nao aprovado.");
    }

    private static OrdemDeServico CriarAguardandoAprovacaoComPeca()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);
        ordem.ConfirmarDiagnosticoConcluido(
            [new ServicoDiagnosticoSnapshot(1000, 150m, 30)],
            [new PecaDiagnosticoSnapshot(1000, 4, 45m)]);
        ordem.ConfirmarOrcamentoGerado(Guid.NewGuid(), 330m, DateTime.UtcNow);
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

    private static OrdemDeServicoHistoricoService CriarHistoricoService()
    {
        var historico = new Mock<IOrdemServicoHistoricoRepository>();
        historico.Setup(item => item.CriarAsync(
                It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()))
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
