using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.Veiculos;

public class VeiculoHandlersTests
{
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;

    public VeiculoHandlersTests()
    {
        _veiculoRepositoryMock = VeiculoRepositoryMockFactory.CreateStrict();
        _clienteRepositoryMock = ClienteRepositoryMockFactory.CreateStrict();
    }

    [Fact]
    public async Task CriarVeiculo_DevePersistirQuandoClienteExistirEPlacaForValida()
    {
        const int clienteId = 1;
        var command = new CriarVeiculoCommand
        {
            Placa = "abc-1234",
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2015,
            CpfCnpj = "47654866801"
        };

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(id: clienteId, cpfCnpj: "47654866801"));

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync("ABC1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo?)null);

        _veiculoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo veiculo, CancellationToken _) => veiculo);

        var handler = new CriarVeiculoCommandHandler(
            _veiculoRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(command, CancellationToken.None);

        resultado.Placa.Should().Be("ABC1234");
        resultado.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public async Task CriarVeiculo_DeveLancarQuandoPlacaJaExistir()
    {
        const int clienteId = 1;
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(id: clienteId, cpfCnpj: "47654866801"));

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync("BRA2E19", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VeiculoMock.Criar(id: 10, placa: "BRA2E19"));

        var handler = new CriarVeiculoCommandHandler(
            _veiculoRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(new CriarVeiculoCommand
        {
            Placa = "BRA2E19",
            Marca = "VW",
            Modelo = "Gol",
            Ano = 2020,
            CpfCnpj = "47654866801"
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
        _veiculoRepositoryMock.Verify(x => x.CriarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterVeiculoPorPlaca_DeveAceitarPlacaMercosulComSeparador()
    {
        const int clienteId = 1;
        var veiculo = VeiculoMock.Criar(
            id: 10,
            placa: "BRA2E19",
            marca: "Volkswagen",
            modelo: "Gol",
            ano: 2020,
            clienteId: clienteId);

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync("BRA2E19", It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);

        var handler = new ObterVeiculoPorPlacaQueryHandler(
            _veiculoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new ObterVeiculoPorPlacaQuery { Placa = "bra-2e19" }, CancellationToken.None);

        resultado.Placa.Should().Be("BRA2E19");
        resultado.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public async Task DeletarVeiculo_DeveLancarQuandoExistiremOrdensDeServicoAtivasVinculadas()
    {
        var veiculo = VeiculoMock.Criar(id: 10, placa: "BRA2E19");

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);
        _veiculoRepositoryMock
            .Setup(x => x.ExisteEmOrdemDeServicoAtivaAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeletarVeiculoCommandHandler(
            _veiculoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(new DeletarVeiculoCommand { Id = veiculo.Id }, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ordens de servico ativas vinculadas*");
    }

    [Fact]
    public async Task DeletarVeiculo_DeveExcluirQuandoNaoExistiremOrdensDeServicoAtivasVinculadas()
    {
        var veiculo = VeiculoMock.Criar(id: 10, placa: "BRA2E19");

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);
        _veiculoRepositoryMock
            .Setup(x => x.ExisteEmOrdemDeServicoAtivaAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _veiculoRepositoryMock
            .Setup(x => x.DeletarAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeletarVeiculoCommandHandler(
            _veiculoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        await handler.Handle(new DeletarVeiculoCommand { Id = veiculo.Id }, CancellationToken.None);

        _veiculoRepositoryMock.Verify(x => x.DeletarAsync(veiculo.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
