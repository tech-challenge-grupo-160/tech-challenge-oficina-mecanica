using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
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
    public async Task CriarServico_DevePersistirServicoValido()
    {
        var command = new CriarServicoCommand
        {
            Nome = "Balanceamento",
            Descricao = "Balanceamento das rodas",
            Preco = 120m,
            TempoEstimado = 45
        };

        _servicoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Servico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Servico servico, CancellationToken _) => servico.WithId(1001));

        var handler = new CriarServicoCommandHandler(
            _servicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(command, CancellationToken.None);

        resultado.Id.Should().Be(1001);
        resultado.Nome.Should().Be(command.Nome);
        resultado.Descricao.Should().Be(command.Descricao);
        resultado.Preco.Should().Be(command.Preco);
        resultado.TempoEstimado.Should().Be(command.TempoEstimado);
    }

    [Fact]
    public async Task AtualizarServico_DeveAtualizarQuandoServicoExistir()
    {
        var servico = ServicoMock.Criar(id: 1000);
        var command = new AtualizarServicoCommand
        {
            Id = servico.Id,
            Nome = "Alinhamento completo",
            Descricao = "Alinhamento e cambagem",
            Preco = 220m,
            TempoEstimado = 60
        };

        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);
        _servicoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Servico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Servico item, CancellationToken _) => item);

        var handler = new AtualizarServicoCommandHandler(
            _servicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(command, CancellationToken.None);

        resultado.Nome.Should().Be(command.Nome);
        resultado.Descricao.Should().Be(command.Descricao);
        resultado.Preco.Should().Be(command.Preco);
        resultado.TempoEstimado.Should().Be(command.TempoEstimado);
    }

    [Fact]
    public async Task AtualizarServico_DeveLancarQuandoServicoNaoExistir()
    {
        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Servico?)null);

        var handler = new AtualizarServicoCommandHandler(
            _servicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(new AtualizarServicoCommand
        {
            Id = 9999,
            Nome = "Servico inexistente",
            Descricao = "Nao deve atualizar",
            Preco = 100m,
            TempoEstimado = 30
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Servico com ID 9999 nao encontrado.");
        _servicoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Servico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterServicoPorId_DeveRetornarServicoQuandoExistir()
    {
        var servico = ServicoMock.Criar(id: 1000, nome: "Diagnostico", preco: 180m);

        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);

        var handler = new ObterServicoPorIdQueryHandler(
            _servicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new ObterServicoPorIdQuery { Id = servico.Id }, CancellationToken.None);

        resultado.Id.Should().Be(servico.Id);
        resultado.Nome.Should().Be("Diagnostico");
        resultado.Preco.Should().Be(180m);
    }

    [Fact]
    public async Task ListarServicos_DeveRetornarTodosOsServicos()
    {
        var servicos = new[]
        {
            ServicoMock.Criar(id: 1000, nome: "Alinhamento"),
            ServicoMock.Criar(id: 1001, nome: "Balanceamento")
        };

        _servicoRepositoryMock
            .Setup(x => x.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(servicos);

        var handler = new ListarServicosQueryHandler(
            _servicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new ListarServicosQuery(), CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Should().Contain(x => x.Id == 1000 && x.Nome == "Alinhamento");
        resultado.Should().Contain(x => x.Id == 1001 && x.Nome == "Balanceamento");
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
