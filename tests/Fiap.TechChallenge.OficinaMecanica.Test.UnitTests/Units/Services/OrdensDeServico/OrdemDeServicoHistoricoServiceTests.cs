using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Services.OrdensDeServico;

public class OrdemDeServicoHistoricoServiceTests
{
    private readonly Mock<IOrdemServicoHistoricoRepository> _historicoRepositoryMock;
    private readonly Mock<IBusinessMetrics> _businessMetricsMock;
    private readonly OrdemDeServicoHistoricoService _service;
    private readonly DateTime _agora = new(2026, 6, 4, 10, 30, 0);

    public OrdemDeServicoHistoricoServiceTests()
    {
        _historicoRepositoryMock = new Mock<IOrdemServicoHistoricoRepository>(MockBehavior.Strict);
        _businessMetricsMock = new Mock<IBusinessMetrics>(MockBehavior.Strict);

        var usuarioAutenticadoServiceMock = new Mock<IUsuarioAutenticadoService>(MockBehavior.Strict);
        usuarioAutenticadoServiceMock
            .Setup(x => x.ObterUsuarioAtual())
            .Returns(new UsuarioAutenticadoInfo
            {
                UsuarioId = "1000",
                UsuarioNome = "unit-test-user",
                ClienteDocumento = "47654866801"
            });

        var clockMock = new Mock<IClock>(MockBehavior.Strict);
        clockMock.Setup(x => x.Now).Returns(_agora);

        _historicoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoHistorico historico, CancellationToken _) => historico);

        _service = new OrdemDeServicoHistoricoService(
            _historicoRepositoryMock.Object,
            usuarioAutenticadoServiceMock.Object,
            clockMock.Object,
            _businessMetricsMock.Object);
    }

    [Fact]
    public async Task RegistrarAsync_NaoDeveEmitirMetrica_QuandoStatusAnteriorEIgualAoStatusNovo()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);

        await _service.RegistrarAsync(
            ordem,
            TipoEventoOrdemServico.ServicoAdicionado,
            StatusOrdemDeServico.EmDiagnostico,
            StatusOrdemDeServico.EmDiagnostico,
            "Servico adicionado.",
            CancellationToken.None);

        _historicoRepositoryMock.Verify(
            x => x.ObterPorOrdemDeServicoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _businessMetricsMock.Verify(
            x => x.RecordOrderStageDuration(It.IsAny<string>(), It.IsAny<double>()),
            Times.Never);
    }

    [Fact]
    public async Task RegistrarAsync_DeveEmitirMetricaComDuracaoCorreta_QuandoHouverTrocaRealDeStatus()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoAprovacao);
        var inicioDiagnostico = _agora.AddMinutes(-15);
        var historico = new[]
        {
            OrdemServicoHistorico.Registrar(
                ordem.Id,
                "1000",
                "unit-test-user",
                StatusOrdemDeServico.Recebida,
                StatusOrdemDeServico.EmDiagnostico,
                TipoEventoOrdemServico.DiagnosticoIniciado,
                "Diagnostico iniciado.",
                inicioDiagnostico),
            OrdemServicoHistorico.Registrar(
                ordem.Id,
                "1000",
                "unit-test-user",
                StatusOrdemDeServico.EmDiagnostico,
                StatusOrdemDeServico.EmDiagnostico,
                TipoEventoOrdemServico.ServicoAdicionado,
                "Servico adicionado.",
                inicioDiagnostico.AddMinutes(5))
        };

        _historicoRepositoryMock
            .Setup(x => x.ObterPorOrdemDeServicoAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(historico);
        _businessMetricsMock
            .Setup(x => x.RecordOrderStageDuration("diagnostico", 900))
            .Verifiable();

        await _service.RegistrarAsync(
            ordem,
            TipoEventoOrdemServico.DiagnosticoFinalizado,
            StatusOrdemDeServico.EmDiagnostico,
            StatusOrdemDeServico.AguardandoAprovacao,
            "Diagnostico finalizado.",
            CancellationToken.None);

        _businessMetricsMock.Verify(
            x => x.RecordOrderStageDuration("diagnostico", 900),
            Times.Once);
    }
}
