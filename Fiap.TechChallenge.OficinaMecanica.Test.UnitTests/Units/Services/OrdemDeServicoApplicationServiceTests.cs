using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Services;

public class OrdemDeServicoApplicationServiceTests
{
    private readonly Mock<IOrdemDeServicoRepository> _ordemRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock;
    private readonly Mock<IServicoRepository> _servicoRepositoryMock;
    private readonly Mock<IPecaRepository> _pecaRepositoryMock;
    private readonly Mock<IOrdemServicoHistoricoRepository> _historicoRepositoryMock;
    private readonly Mock<IUsuarioAutenticadoService> _usuarioAutenticadoServiceMock;
    private readonly OrdemDeServicoApplicationService _service;

    public OrdemDeServicoApplicationServiceTests()
    {
        _ordemRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _clienteRepositoryMock = new Mock<IClienteRepository>(MockBehavior.Strict);
        _veiculoRepositoryMock = new Mock<IVeiculoRepository>(MockBehavior.Strict);
        _servicoRepositoryMock = new Mock<IServicoRepository>(MockBehavior.Strict);
        _pecaRepositoryMock = new Mock<IPecaRepository>(MockBehavior.Strict);
        _historicoRepositoryMock = new Mock<IOrdemServicoHistoricoRepository>(MockBehavior.Strict);
        _usuarioAutenticadoServiceMock = new Mock<IUsuarioAutenticadoService>(MockBehavior.Strict);

        _usuarioAutenticadoServiceMock
            .Setup(x => x.ObterUsuarioAtual())
            .Returns(new UsuarioAutenticadoInfo
            {
                UsuarioId = "1000",
                UsuarioNome = "unit-test-user"
            });
        _historicoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoHistorico historico, CancellationToken _) => historico);

        _service = new OrdemDeServicoApplicationService(
            _ordemRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _servicoRepositoryMock.Object,
            _pecaRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _usuarioAutenticadoServiceMock.Object,
            NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task CriarOrdemDeServicoAsync_DeveAbrirOsEmRecebida()
    {
        var dto = CriarOrdemDeServicoDtoMock.Criar(clienteId: 1, veiculoId: 10);
        var cliente = ClienteMock.Criar(id: 1);
        var veiculo = VeiculoMock.Criar(id: 10, clienteId: 1);

        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(dto.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(dto.VeiculoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);
        _ordemRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico ordem, CancellationToken _) =>
            {
                ordem.Id = 3002;
                return ordem;
            });
        _ordemRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico ordem, CancellationToken _) => ordem);

        var resultado = await _service.CriarOrdemDeServicoAsync(dto, CancellationToken.None);

        resultado.Status.Should().Be(nameof(StatusOrdemDeServico.Recebida));
        resultado.Numero.Should().Be($"OS-{resultado.DataAbertura:yyyyMMdd}-3002");
        resultado.ValorTotal.Should().Be(0);
        _historicoRepositoryMock.Verify(
            x => x.CriarAsync(
                It.Is<OrdemServicoHistorico>(h =>
                    h.OrdemDeServicoId == 3002 &&
                    h.UsuarioId == "1000" &&
                    h.UsuarioNome == "unit-test-user" &&
                    h.StatusAnterior == null &&
                    h.StatusNovo == StatusOrdemDeServico.Recebida &&
                    h.TipoEvento == TipoEventoOrdemServico.OrdemCriada),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FinalizarDiagnosticoAsync_DeveLancarQuandoNaoHouverServico()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.FinalizarDiagnosticoAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ao menos um servico*");
    }

    [Fact]
    public async Task FinalizarDiagnosticoAsync_DeveLancarQuandoOrcamentoForZero()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);
        ordem.Servicos.Add(new OrdemDeServicoServico
        {
            OrdemDeServicoId = ordem.Id,
            ServicoId = 1000,
            Preco = 0,
            TempoEstimado = 30
        });

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.FinalizarDiagnosticoAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nao pode ser zerado*");
    }

    [Fact]
    public async Task FluxoCompleto_DeveAtualizarStatusEDatasCorretamente()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.Recebida, clienteId: 1, veiculoId: 10);
        var servico = ServicoMock.Criar(id: 1000, preco: 150m);
        var peca = PecaMock.Criar(id: 1000, preco: 45m, quantidadeEstoque: 5);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _ordemRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico os, CancellationToken _) => os);
        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);

        var emDiagnostico = await _service.IniciarDiagnosticoAsync(ordem.Id, CancellationToken.None);
        var comServico = await _service.AdicionarServicoAsync(ordem.Id, new AdicionarServicoAOrdemDto { ServicoId = servico.Id }, CancellationToken.None);
        var comPeca = await _service.AdicionarPecaAsync(ordem.Id, new AdicionarPecaAOrdemDto { PecaId = peca.Id, Quantidade = 1 }, CancellationToken.None);
        var aguardandoAprovacao = await _service.FinalizarDiagnosticoAsync(ordem.Id, CancellationToken.None);
        var emExecucao = await _service.AprovarAsync(ordem.Id, CancellationToken.None);
        var finalizada = await _service.FinalizarAsync(ordem.Id, CancellationToken.None);
        var pagamentoRegistrado = await _service.RegistrarPagamentoAsync(ordem.Id, CancellationToken.None);
        var entregue = await _service.EntregarAsync(ordem.Id, CancellationToken.None);

        emDiagnostico.Status.Should().Be(nameof(StatusOrdemDeServico.EmDiagnostico));
        comServico.ValorTotal.Should().Be(150m);
        comPeca.ValorTotal.Should().Be(195m);
        aguardandoAprovacao.Status.Should().Be(nameof(StatusOrdemDeServico.AguardandoAprovacao));
        aguardandoAprovacao.OrcamentoEnviadoEm.Should().NotBeNull();
        emExecucao.Status.Should().Be(nameof(StatusOrdemDeServico.EmExecucao));
        finalizada.Status.Should().Be(nameof(StatusOrdemDeServico.Finalizada));
        finalizada.DataFinalizacao.Should().NotBeNull();
        pagamentoRegistrado.DataPagamento.Should().NotBeNull();
        entregue.Status.Should().Be(nameof(StatusOrdemDeServico.Entregue));
        entregue.DataConclusao.Should().NotBeNull();
        _historicoRepositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()),
            Times.Exactly(8));
    }

    [Fact]
    public async Task EntregarAsync_DeveLancarQuandoPagamentoNaoTiverSidoRegistrado()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.Finalizada);
        ordem.DataFinalizacao = DateTime.UtcNow;

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.EntregarAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*apos o pagamento*");
    }
}
