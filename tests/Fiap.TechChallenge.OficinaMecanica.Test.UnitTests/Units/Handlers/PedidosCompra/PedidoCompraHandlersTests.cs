using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.PedidosCompra;

public class PedidoCompraHandlersTests
{
    private readonly Mock<IPedidoCompraRepository> _pedidoCompraRepositoryMock;
    private readonly Mock<IOrdemDeServicoRepository> _ordemDeServicoRepositoryMock;
    private readonly Mock<IPecaRepository> _pecaRepositoryMock;
    private readonly Mock<IMovimentacaoEstoqueRepository> _movimentacaoEstoqueRepositoryMock;
    private readonly Mock<IOrdemServicoHistoricoRepository> _historicoRepositoryMock;
    private readonly Mock<IUsuarioAutenticadoService> _usuarioAutenticadoServiceMock;
    private readonly Mock<ITransactionManager> _transactionManagerMock;
    private readonly Mock<IClock> _clockMock;

    public PedidoCompraHandlersTests()
    {
        _pedidoCompraRepositoryMock = new Mock<IPedidoCompraRepository>(MockBehavior.Strict);
        _ordemDeServicoRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _pecaRepositoryMock = new Mock<IPecaRepository>(MockBehavior.Strict);
        _movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>(MockBehavior.Strict);
        _historicoRepositoryMock = new Mock<IOrdemServicoHistoricoRepository>(MockBehavior.Strict);
        _usuarioAutenticadoServiceMock = new Mock<IUsuarioAutenticadoService>(MockBehavior.Strict);
        _transactionManagerMock = new Mock<ITransactionManager>(MockBehavior.Strict);
        _clockMock = new Mock<IClock>(MockBehavior.Strict);
        _clockMock.Setup(x => x.Now).Returns(new DateTime(2026, 6, 4, 10, 0, 0));
    }

    [Fact]
    public async Task CriarPedidoCompra_DeveCriarPedidoCompraManual()
    {
        var ordem = OrdemDeServicoMock.Criar(
            id: 3001,
            status: StatusOrdemDeServico.AguardandoEstoque,
            numero: "OS-20260423-3001");
        var peca = PecaMock.Criar(id: 1000);

        _transactionManagerMock
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<PedidoCompraResult>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<PedidoCompraResult>> action, CancellationToken ct) => action(ct));
        _ordemDeServicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pedidoCompraRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<PedidoCompra>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoCompra pedido, CancellationToken _) => pedido.WithId(50));
        ConfigurarUsuarioAtual();
        _historicoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoHistorico historico, CancellationToken _) => historico);

        var handler = CriarHandler();

        var resultado = await handler.Handle(
            new CriarPedidoCompraCommand
            {
                OrdemDeServicoId = ordem.Id,
                PecaId = peca.Id,
                QuantidadeSolicitada = 3,
                Observacao = "Compra manual"
            },
            CancellationToken.None);

        resultado.Id.Should().Be(50);
        resultado.OrdemDeServicoId.Should().Be(ordem.Id);
        resultado.PecaId.Should().Be(peca.Id);
        resultado.QuantidadeSolicitada.Should().Be(3);
        resultado.Status.Should().Be(nameof(StatusPedidoCompra.Pendente));
        resultado.Observacao.Should().Be("Compra manual");
    }

    [Fact]
    public async Task ListarPedidosCompra_DeveRetornarPedidosPaginados()
    {
        var pedidos = new[]
        {
            PedidoCompra.Criar(3001, 1000, 2, new DateTime(2026, 4, 23, 10, 0, 0), "Pedido 1")
                .WithId(1)
        };
        pedidos[0].VincularPeca(PecaMock.Criar(id: 1000));

        _pedidoCompraRepositoryMock
            .Setup(x => x.ContarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        _pedidoCompraRepositoryMock
            .Setup(x => x.ObterPaginadoAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedidos);

        var handler = new ListarPedidosCompraQueryHandler(
            _pedidoCompraRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new ListarPedidosCompraQuery { Page = 1, PageSize = 1 }, CancellationToken.None);

        resultado.Items.Should().HaveCount(1);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(1);
        resultado.TotalItems.Should().Be(3);
        resultado.TotalPages.Should().Be(3);
        resultado.Items.Single().NomePeca.Should().Be("Pastilha de Freio");
        resultado.Items.Single().Status.Should().Be(nameof(StatusPedidoCompra.Pendente));
    }

    [Fact]
    public async Task ListarPedidosCompraPorOrdem_DeveRetornarPedidosDaOrdem()
    {
        var pedidos = new[]
        {
            PedidoCompra.Criar(3001, 1000, 2, new DateTime(2026, 4, 23, 10, 0, 0), "Pedido 1")
                .WithId(1)
        };
        pedidos[0].VincularPeca(PecaMock.Criar(id: 1000, nome: "Filtro de oleo"));

        _pedidoCompraRepositoryMock
            .Setup(x => x.ObterPorOrdemDeServicoAsync(3001, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedidos);

        var handler = new ListarPedidosCompraPorOrdemQueryHandler(
            _pedidoCompraRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(
            new ListarPedidosCompraPorOrdemQuery { OrdemDeServicoId = 3001 },
            CancellationToken.None);

        resultado.Should().ContainSingle(x =>
            x.Id == 1 &&
            x.OrdemDeServicoId == 3001 &&
            x.PecaId == 1000 &&
            x.NomePeca == "Filtro de oleo");
    }

    [Fact]
    public async Task ReceberPedidoCompra_DeveAtualizarPedidoPecaEMovimentacaoEstoque()
    {
        var pedido = PedidoCompra.Criar(3001, 1000, 5, new DateTime(2026, 6, 4, 9, 0, 0), "Pedido")
            .WithId(50);
        var peca = PecaMock.Criar(id: 1000, quantidadeEstoque: 7);

        _transactionManagerMock
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<PedidoCompraResult>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<PedidoCompraResult>> action, CancellationToken ct) => action(ct));
        _pedidoCompraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(pedido.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pedidoCompraRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<PedidoCompra>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoCompra pedidoAtualizado, CancellationToken _) => pedidoAtualizado);
        _pecaRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Peca>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Peca pecaAtualizada, CancellationToken _) => pecaAtualizada);
        _movimentacaoEstoqueRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovimentacaoEstoque movimentacao, CancellationToken _) => movimentacao);
        ConfigurarUsuarioAtual();
        _historicoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoHistorico historico, CancellationToken _) => historico);

        var handler = ReceberHandler();

        var resultado = await handler.Handle(
            new ReceberPedidoCompraCommand
            {
                PedidoCompraId = pedido.Id,
                QuantidadeRecebida = 5
            },
            CancellationToken.None);

        resultado.Status.Should().Be(nameof(StatusPedidoCompra.Recebido));
        resultado.QuantidadeRecebida.Should().Be(5);
        peca.QuantidadeEstoque.Should().Be(12);

        _pedidoCompraRepositoryMock.Verify(x => x.AtualizarAsync(pedido, It.IsAny<CancellationToken>()), Times.Once);
        _pecaRepositoryMock.Verify(x => x.AtualizarAsync(peca, It.IsAny<CancellationToken>()), Times.Once);
        _movimentacaoEstoqueRepositoryMock.Verify(
            x => x.CriarAsync(
                It.Is<MovimentacaoEstoque>(m =>
                    m.PecaId == peca.Id &&
                    m.PedidoCompraId == pedido.Id &&
                    m.QuantidadeAnterior == 7 &&
                    m.QuantidadePosterior == 12 &&
                    m.TipoMovimentacao == TipoMovimentacaoEstoque.EntradaPorPedidoCompra),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _historicoRepositoryMock.Verify(x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private CriarPedidoCompraCommandHandler CriarHandler()
    {
        return new CriarPedidoCompraCommandHandler(
            _pedidoCompraRepositoryMock.Object,
            _ordemDeServicoRepositoryMock.Object,
            _pecaRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _usuarioAutenticadoServiceMock.Object,
            _transactionManagerMock.Object,
            _clockMock.Object,
            NullLoggerFactory.Instance);
    }

    private ReceberPedidoCompraCommandHandler ReceberHandler()
    {
        return new ReceberPedidoCompraCommandHandler(
            _pedidoCompraRepositoryMock.Object,
            _pecaRepositoryMock.Object,
            _movimentacaoEstoqueRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _usuarioAutenticadoServiceMock.Object,
            _transactionManagerMock.Object,
            _clockMock.Object,
            NullLoggerFactory.Instance);
    }

    private void ConfigurarUsuarioAtual()
    {
        _usuarioAutenticadoServiceMock
            .Setup(x => x.ObterUsuarioAtual())
            .Returns(new UsuarioAutenticadoInfo
            {
                UsuarioId = "1000",
                UsuarioNome = "unit-test-user"
            });
    }
}
