using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Pecas;
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
