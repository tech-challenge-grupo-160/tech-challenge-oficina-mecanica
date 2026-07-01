using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.Pecas;

public class PecaHandlersTests
{
    private readonly Mock<IPecaRepository> _pecaRepositoryMock;

    public PecaHandlersTests()
    {
        _pecaRepositoryMock = new Mock<IPecaRepository>(MockBehavior.Strict);
    }

    [Fact]
    public async Task CriarPeca_DevePersistirPecaValida()
    {
        var command = new CriarPecaCommand
        {
            Nome = "Filtro de oleo",
            Marca = "Tecfil",
            Modelo = "PSL55",
            Preco = 35m,
            QuantidadeEstoque = 12
        };

        _pecaRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Peca>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Peca peca, CancellationToken _) => peca.WithId(1001));

        var handler = new CriarPecaCommandHandler(
            _pecaRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(command, CancellationToken.None);

        resultado.Id.Should().Be(1001);
        resultado.Nome.Should().Be(command.Nome);
        resultado.Marca.Should().Be(command.Marca);
        resultado.Modelo.Should().Be(command.Modelo);
        resultado.Preco.Should().Be(command.Preco);
        resultado.QuantidadeEstoque.Should().Be(command.QuantidadeEstoque);
    }

    [Fact]
    public async Task AtualizarPeca_DeveAtualizarQuandoPecaExistir()
    {
        var peca = PecaMock.Criar(id: 1000);
        var command = new AtualizarPecaCommand
        {
            Id = peca.Id,
            Nome = "Pastilha premium",
            Marca = "Bosch",
            Modelo = "BP-900",
            Preco = 95m,
            QuantidadeEstoque = 4
        };

        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pecaRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Peca>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Peca item, CancellationToken _) => item);

        var handler = new AtualizarPecaCommandHandler(
            _pecaRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(command, CancellationToken.None);

        resultado.Nome.Should().Be(command.Nome);
        resultado.Marca.Should().Be(command.Marca);
        resultado.Modelo.Should().Be(command.Modelo);
        resultado.Preco.Should().Be(command.Preco);
        resultado.QuantidadeEstoque.Should().Be(command.QuantidadeEstoque);
    }

    [Fact]
    public async Task AtualizarPeca_DeveLancarQuandoPecaNaoExistir()
    {
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Peca?)null);

        var handler = new AtualizarPecaCommandHandler(
            _pecaRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(new AtualizarPecaCommand
        {
            Id = 9999,
            Nome = "Peca inexistente",
            Marca = "Marca",
            Modelo = "Modelo",
            Preco = 10m,
            QuantidadeEstoque = 1
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Peca com ID 9999 nao encontrada.");
        _pecaRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Peca>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterPecaPorId_DeveRetornarPecaQuandoExistir()
    {
        var peca = PecaMock.Criar(id: 1000, nome: "Filtro de ar", marca: "Tecfil");

        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);

        var handler = new ObterPecaPorIdQueryHandler(
            _pecaRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new ObterPecaPorIdQuery { Id = peca.Id }, CancellationToken.None);

        resultado.Id.Should().Be(peca.Id);
        resultado.Nome.Should().Be("Filtro de ar");
        resultado.Marca.Should().Be("Tecfil");
    }

    [Fact]
    public async Task ListarPecas_DeveRetornarTodasAsPecas()
    {
        var pecas = new[]
        {
            PecaMock.Criar(id: 1000, nome: "Pastilha"),
            PecaMock.Criar(id: 1001, nome: "Filtro")
        };

        _pecaRepositoryMock
            .Setup(x => x.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pecas);

        var handler = new ListarPecasQueryHandler(
            _pecaRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new ListarPecasQuery(), CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Should().Contain(x => x.Id == 1000 && x.Nome == "Pastilha");
        resultado.Should().Contain(x => x.Id == 1001 && x.Nome == "Filtro");
    }

    [Fact]
    public async Task DeletarPeca_DeveLancarQuandoExistiremOrdensDeServicoAtivasVinculadas()
    {
        var peca = PecaMock.Criar(id: 1000);

        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pecaRepositoryMock
            .Setup(x => x.ExisteEmOrdemDeServicoAtivaAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeletarPecaCommandHandler(
            _pecaRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(new DeletarPecaCommand { Id = peca.Id }, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ordens de servico ativas vinculadas*");
    }

    [Fact]
    public async Task DeletarPeca_DeveExcluirQuandoNaoExistiremOrdensDeServicoAtivasVinculadas()
    {
        var peca = PecaMock.Criar(id: 1000);

        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pecaRepositoryMock
            .Setup(x => x.ExisteEmOrdemDeServicoAtivaAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _pecaRepositoryMock
            .Setup(x => x.DeletarAsync(peca.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeletarPecaCommandHandler(
            _pecaRepositoryMock.Object,
            NullLoggerFactory.Instance);

        await handler.Handle(new DeletarPecaCommand { Id = peca.Id }, CancellationToken.None);

        _pecaRepositoryMock.Verify(x => x.DeletarAsync(peca.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
