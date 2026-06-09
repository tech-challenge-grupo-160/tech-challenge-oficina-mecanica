using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.Servicos;

public class ServicoHandlersTests
{
    private readonly Mock<IServicoRepository> _servicoRepositoryMock;

    public ServicoHandlersTests()
    {
        _servicoRepositoryMock = new Mock<IServicoRepository>(MockBehavior.Strict);
    }

    [Fact]
    public async Task DeletarServico_DeveLancarQuandoExistiremOrdensDeServicoAtivasVinculadas()
    {
        var servico = ServicoMock.Criar(id: 1000);

        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);
        _servicoRepositoryMock
            .Setup(x => x.ExisteEmOrdemDeServicoAtivaAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeletarServicoCommandHandler(
            _servicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(new DeletarServicoCommand { Id = servico.Id }, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ordens de servico ativas vinculadas*");
    }

    [Fact]
    public async Task DeletarServico_DeveExcluirQuandoNaoExistiremOrdensDeServicoAtivasVinculadas()
    {
        var servico = ServicoMock.Criar(id: 1000);

        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);
        _servicoRepositoryMock
            .Setup(x => x.ExisteEmOrdemDeServicoAtivaAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _servicoRepositoryMock
            .Setup(x => x.DeletarAsync(servico.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeletarServicoCommandHandler(
            _servicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        await handler.Handle(new DeletarServicoCommand { Id = servico.Id }, CancellationToken.None);

        _servicoRepositoryMock.Verify(x => x.DeletarAsync(servico.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
