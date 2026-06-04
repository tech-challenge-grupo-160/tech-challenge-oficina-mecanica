using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Services;

public class PecaApplicationServiceTests
{
    private readonly Mock<IPecaRepository> _pecaRepositoryMock;
    private readonly PecaApplicationService _service;

    public PecaApplicationServiceTests()
    {
        _pecaRepositoryMock = new Mock<IPecaRepository>(MockBehavior.Strict);
        _service = new PecaApplicationService(_pecaRepositoryMock.Object, NullLoggerFactory.Instance);
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

        var acao = () => _service.DeletarPecaAsync(peca.Id, CancellationToken.None);

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

        await _service.DeletarPecaAsync(peca.Id, CancellationToken.None);

        _pecaRepositoryMock.Verify(x => x.DeletarAsync(peca.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
