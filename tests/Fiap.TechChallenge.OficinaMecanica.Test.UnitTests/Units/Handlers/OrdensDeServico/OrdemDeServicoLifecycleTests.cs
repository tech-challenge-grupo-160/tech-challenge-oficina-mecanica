using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.OrdensDeServico;

public class OrdemDeServicoLifecycleTests
{
    [Fact]
    public void Abertura_DevePersistirSnapshotsSemEntidadesDeCatalogoLocais()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.Recebida);

        ordem.AdicionarServicoNaAberturaComEvento(1000, "Alinhamento", 150m, 30);
        ordem.AdicionarPecaNaAberturaComEvento(1000, "Pastilha", 45m, 2);

        ordem.Servicos.Should().ContainSingle(item =>
            item.ServicoId == 1000 && item.Preco == 150m && item.TempoEstimado == 30);
        ordem.Pecas.Should().ContainSingle(item =>
            item.PecaId == 1000 && item.Preco == 45m && item.Quantidade == 2);
        ordem.ValorTotal.Should().Be(240m);
    }

    [Fact]
    public void DiagnosticoConcluido_DeveSubstituirSnapshotsERecalcularTotal()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);
        ordem.Servicos.Add(OrdemDeServicoServico.Criar(ordem.Id, 1000, 150m, 30));
        ordem.Pecas.Add(OrdemDeServicoPeca.Criar(ordem.Id, 1000, 2, 45m));

        var evento = ordem.ConfirmarDiagnosticoConcluido(
            [new ServicoDiagnosticoSnapshot(1001, 180m, 45)],
            [new PecaDiagnosticoSnapshot(1002, 1, 39.90m)]);

        ordem.Status.Should().Be(StatusOrdemDeServico.AguardandoOrcamento);
        ordem.Servicos.Should().ContainSingle(item => item.ServicoId == 1001);
        ordem.Pecas.Should().ContainSingle(item => item.PecaId == 1002 && item.Quantidade == 1);
        ordem.ValorTotal.Should().Be(219.90m);
        evento.TipoEvento.Should().Be(TipoEventoOrdemServico.DiagnosticoFinalizado);
    }

    [Fact]
    public void DiagnosticoConcluido_DeveExigirAoMenosUmServico()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);

        var acao = () => ordem.ConfirmarDiagnosticoConcluido([], []);

        acao.Should().Throw<InvalidOperationException>().WithMessage("*ao menos um servico*");
    }

    [Fact]
    public void OrcamentoGerado_DeveAtualizarEstadoEData()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);
        ordem.ConfirmarDiagnosticoConcluido(
            [new ServicoDiagnosticoSnapshot(1001, 180m, 45)],
            [new PecaDiagnosticoSnapshot(1002, 1, 39.90m)]);
        var data = new DateTime(2026, 11, 8, 10, 0, 0);

        ordem.ConfirmarOrcamentoGerado(Guid.NewGuid(), 219.90m, data);

        ordem.Status.Should().Be(StatusOrdemDeServico.AguardandoAprovacao);
        ordem.OrcamentoEnviadoEm.Should().Be(data);
    }

    [Fact]
    public void FluxoFinal_DeveExigirPagamentoAntesDaEntrega()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmExecucao);
        var finalizacao = new DateTime(2026, 11, 8, 11, 0, 0);
        ordem.FinalizarServico(finalizacao);

        var entregaSemPagamento = () => ordem.Entregar(finalizacao.AddMinutes(10));
        entregaSemPagamento.Should().Throw<InvalidOperationException>().WithMessage("*pagamento*");

        var pagamento = finalizacao.AddMinutes(20);
        var entrega = pagamento.AddMinutes(10);
        ordem.RegistrarPagamento(pagamento);
        ordem.Entregar(entrega);

        ordem.Status.Should().Be(StatusOrdemDeServico.Entregue);
        ordem.DataFinalizacao.Should().Be(finalizacao);
        ordem.DataPagamento.Should().Be(pagamento);
        ordem.DataConclusao.Should().Be(entrega);
    }

    [Fact]
    public void Cancelamento_DeveRegistrarMotivo()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.Recebida);

        var evento = ordem.CancelarComEvento("Cliente desistiu.");

        ordem.Status.Should().Be(StatusOrdemDeServico.Cancelada);
        ordem.MotivoCancelamento.Should().Be("Cliente desistiu.");
        evento.TipoEvento.Should().Be(TipoEventoOrdemServico.OrdemCancelada);
    }
}
