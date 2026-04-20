using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Services;

public class VeiculoApplicationServiceTests
{
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly VeiculoApplicationService _service;

    public VeiculoApplicationServiceTests()
    {
        _veiculoRepositoryMock = VeiculoRepositoryMockFactory.CreateStrict();
        _clienteRepositoryMock = ClienteRepositoryMockFactory.CreateStrict();
        _service = new VeiculoApplicationService(_veiculoRepositoryMock.Object, _clienteRepositoryMock.Object, NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task CriarVeiculo_DevePersistirQuandoClienteExistirEPlacaForValida()
    {
        const int clienteId = 1;
        var dto = CriarVeiculoDtoMock.Criar(
            placa: "abc-1234",
            marca: "Fiat",
            modelo: "Uno",
            ano: 2015,
            cpfCnpj: "47654866801");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(id: clienteId, cpfCnpj: "47654866801"));

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync("ABC1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo?)null);

        _veiculoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo veiculo, CancellationToken _) => veiculo);

        var resultado = await _service.CriarVeiculoAsync(dto, CancellationToken.None);

        resultado.Placa.Should().Be("ABC1234");
        resultado.ClienteId.Should().Be(clienteId);

        _clienteRepositoryMock.Verify(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()), Times.Once);
        _veiculoRepositoryMock.Verify(x => x.ObterPorPlacaAsync("ABC1234", It.IsAny<CancellationToken>()), Times.Once);
        _veiculoRepositoryMock.Verify(
            x => x.CriarAsync(It.Is<Veiculo>(v => v.Placa == "ABC1234" && v.ClienteId == clienteId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CriarVeiculo_DeveLancarQuandoPlacaJaExistir()
    {
        const int clienteId = 1;
        var dto = CriarVeiculoDtoMock.Criar(
            placa: "BRA2E19",
            marca: "VW",
            modelo: "Gol",
            ano: 2020,
            cpfCnpj: "47654866801");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(id: clienteId, cpfCnpj: "47654866801"));

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync("BRA2E19", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VeiculoMock.Criar(id: 10, placa: "BRA2E19"));

        var acao = () => _service.CriarVeiculoAsync(dto, CancellationToken.None);

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

        var resultado = await _service.ObterVeiculoPorPlacaAsync("bra-2e19", CancellationToken.None);

        resultado.Placa.Should().Be("BRA2E19");
        resultado.ClienteId.Should().Be(clienteId);
    }
}
