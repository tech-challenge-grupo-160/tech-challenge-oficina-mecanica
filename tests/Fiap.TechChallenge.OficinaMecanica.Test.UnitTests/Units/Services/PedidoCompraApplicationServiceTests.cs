using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Services;

public class PedidoCompraApplicationServiceTests
{
    private readonly Mock<IPedidoCompraRepository> _pedidoCompraRepositoryMock;
    private readonly Mock<IOrdemDeServicoRepository> _ordemDeServicoRepositoryMock;
    private readonly Mock<IPecaRepository> _pecaRepositoryMock;
    private readonly Mock<IMovimentacaoEstoqueRepository> _movimentacaoEstoqueRepositoryMock;
    private readonly Mock<IOrdemServicoHistoricoRepository> _historicoRepositoryMock;
    private readonly Mock<IUsuarioAutenticadoService> _usuarioAutenticadoServiceMock;
    private readonly Mock<ITransactionManager> _transactionManagerMock;
    private readonly PedidoCompraApplicationService _service;

    public PedidoCompraApplicationServiceTests()
    {
        _pedidoCompraRepositoryMock = new Mock<IPedidoCompraRepository>(MockBehavior.Strict);
        _ordemDeServicoRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _pecaRepositoryMock = new Mock<IPecaRepository>(MockBehavior.Strict);
        _movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>(MockBehavior.Strict);
        _historicoRepositoryMock = new Mock<IOrdemServicoHistoricoRepository>(MockBehavior.Strict);
        _usuarioAutenticadoServiceMock = new Mock<IUsuarioAutenticadoService>(MockBehavior.Strict);
        _transactionManagerMock = new Mock<ITransactionManager>(MockBehavior.Strict);

        _service = new PedidoCompraApplicationService(
            _pedidoCompraRepositoryMock.Object,
            _ordemDeServicoRepositoryMock.Object,
            _pecaRepositoryMock.Object,
            _movimentacaoEstoqueRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _usuarioAutenticadoServiceMock.Object,
            _transactionManagerMock.Object,
            NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarPedidoCompraManual()
    {
        var ordem = new OrdemDeServico
        {
            Id = 3001,
            Numero = "OS-20260423-3001",
            Status = StatusOrdemDeServico.AguardandoEstoque
        };
        var peca = new Peca
        {
            Id = 1000,
            Nome = "Pastilha de Freio",
            Marca = "Cobreq",
            Modelo = "N-1234"
        };

        _transactionManagerMock
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<PedidoCompraDto>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<PedidoCompraDto>> action, CancellationToken ct) => action(ct));
        _ordemDeServicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pedidoCompraRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<PedidoCompra>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoCompra pedido, CancellationToken _) =>
            {
                pedido.Id = 50;
                return pedido;
            });
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

        var resultado = await _service.CriarAsync(
            new CriarPedidoCompraDto
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
    public async Task ListarAsync_DeveRetornarPedidosDeCompraPaginados()
    {
        var pedidos = new[]
        {
            new PedidoCompra
            {
                Id = 1,
                OrdemDeServicoId = 3001,
                PecaId = 1000,
                QuantidadeSolicitada = 2,
                QuantidadeRecebida = 0,
                Status = StatusPedidoCompra.Pendente,
                DataSolicitacao = new DateTime(2026, 4, 23, 10, 0, 0),
                Observacao = "Pedido 1",
                Peca = new Peca { Id = 1000, Nome = "Pastilha de Freio", Marca = "Cobreq", Modelo = "N-1234" }
            }
        };

        _pedidoCompraRepositoryMock
            .Setup(x => x.ContarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        _pedidoCompraRepositoryMock
            .Setup(x => x.ObterPaginadoAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedidos);

        var resultado = await _service.ListarAsync(1, 1, CancellationToken.None);

        resultado.Items.Should().HaveCount(1);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(1);
        resultado.TotalItems.Should().Be(3);
        resultado.TotalPages.Should().Be(3);
        resultado.Items.Single().NomePeca.Should().Be("Pastilha de Freio");
        resultado.Items.Single().Status.Should().Be(nameof(StatusPedidoCompra.Pendente));
    }
}
