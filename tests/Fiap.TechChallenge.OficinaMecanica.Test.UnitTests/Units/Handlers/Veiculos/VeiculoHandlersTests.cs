using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
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
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(id: clienteId, cpfCnpj: "47654866801"));

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync(PlacaVeiculo.Parse("ABC1234"), It.IsAny<CancellationToken>()))
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
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(id: clienteId, cpfCnpj: "47654866801"));

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync(PlacaVeiculo.Parse("BRA2E19"), It.IsAny<CancellationToken>()))
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
    public async Task CriarVeiculoParaCliente_DevePersistirQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(id: 1, cpfCnpj: "47654866801");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ObterPorPlacaAsync(PlacaVeiculo.Parse("DEF1234"), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo?)null);
        _veiculoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo veiculo, CancellationToken _) => veiculo.WithId(11));

        var handler = new CriarVeiculoParaClienteCommandHandler(
            _veiculoRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new CriarVeiculoParaClienteCommand
        {
            CpfCnpj = "476.548.668-01",
            Placa = "def-1234",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2022
        }, CancellationToken.None);

        resultado.Id.Should().Be(11);
        resultado.Placa.Should().Be("DEF1234");
        resultado.ClienteId.Should().Be(cliente.Id);
    }

    [Fact]
    public async Task AtualizarVeiculo_DeveAtualizarDadosQuandoVeiculoExistir()
    {
        var veiculo = VeiculoMock.Criar(id: 10, placa: "BRA2E19", marca: "VW", modelo: "Gol", ano: 2020);

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);
        _veiculoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo item, CancellationToken _) => item);

        var handler = new AtualizarVeiculoCommandHandler(_veiculoRepositoryMock.Object);

        var resultado = await handler.Handle(new AtualizarVeiculoCommand
        {
            Id = veiculo.Id,
            Marca = "Volkswagen",
            Modelo = "Polo",
            Ano = 2023
        }, CancellationToken.None);

        resultado.Marca.Should().Be("Volkswagen");
        resultado.Modelo.Should().Be("Polo");
        resultado.Ano.Should().Be(2023);
    }

    [Fact]
    public async Task AtualizarVeiculo_DeveLancarQuandoVeiculoNaoExistir()
    {
        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo?)null);

        var handler = new AtualizarVeiculoCommandHandler(_veiculoRepositoryMock.Object);

        var acao = () => handler.Handle(new AtualizarVeiculoCommand
        {
            Id = 9999,
            Marca = "Marca",
            Modelo = "Modelo",
            Ano = 2020
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Veiculo com ID 9999 nao encontrado.");
        _veiculoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterVeiculoPorId_DeveRetornarVeiculoQuandoExistir()
    {
        var veiculo = VeiculoMock.Criar(id: 10, placa: "BRA2E19");

        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);

        var handler = new ObterVeiculoPorIdQueryHandler(_veiculoRepositoryMock.Object);

        var resultado = await handler.Handle(new ObterVeiculoPorIdQuery { Id = veiculo.Id }, CancellationToken.None);

        resultado.Id.Should().Be(veiculo.Id);
        resultado.Placa.Should().Be("BRA2E19");
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
            .Setup(x => x.ObterPorPlacaAsync(PlacaVeiculo.Parse("BRA2E19"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);

        var handler = new ObterVeiculoPorPlacaQueryHandler(
            _veiculoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(new ObterVeiculoPorPlacaQuery { Placa = "bra-2e19" }, CancellationToken.None);

        resultado.Placa.Should().Be("BRA2E19");
        resultado.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public async Task ListarVeiculos_DeveRetornarTodosOsVeiculos()
    {
        var veiculos = new[]
        {
            VeiculoMock.Criar(id: 10, placa: "ABC1234"),
            VeiculoMock.Criar(id: 11, placa: "BRA2E19")
        };

        _veiculoRepositoryMock
            .Setup(x => x.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculos);

        var handler = new ListarVeiculosQueryHandler(_veiculoRepositoryMock.Object);

        var resultado = await handler.Handle(new ListarVeiculosQuery(), CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Should().Contain(x => x.Id == 10 && x.Placa == "ABC1234");
        resultado.Should().Contain(x => x.Id == 11 && x.Placa == "BRA2E19");
    }

    [Fact]
    public async Task ListarVeiculosPorCliente_DeveRetornarVeiculosQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(id: 1);
        var veiculos = new[]
        {
            VeiculoMock.Criar(id: 10, placa: "ABC1234", clienteId: cliente.Id)
        };

        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ObterPorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculos);

        var handler = new ListarVeiculosPorClienteQueryHandler(
            _veiculoRepositoryMock.Object,
            _clienteRepositoryMock.Object);

        var resultado = await handler.Handle(new ListarVeiculosPorClienteQuery { ClienteId = cliente.Id }, CancellationToken.None);

        resultado.Should().ContainSingle(x => x.Id == 10 && x.ClienteId == cliente.Id);
    }

    [Fact]
    public async Task ListarVeiculosPorDocumentoCliente_DeveRetornarVeiculosQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(id: 1, cpfCnpj: "47654866801");
        var veiculos = new[]
        {
            VeiculoMock.Criar(id: 10, placa: "ABC1234", clienteId: cliente.Id)
        };

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ObterPorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculos);

        var handler = new ListarVeiculosPorDocumentoClienteQueryHandler(
            _veiculoRepositoryMock.Object,
            _clienteRepositoryMock.Object);

        var resultado = await handler.Handle(
            new ListarVeiculosPorDocumentoClienteQuery { CpfCnpj = "476.548.668-01" },
            CancellationToken.None);

        resultado.Should().ContainSingle(x => x.Id == 10 && x.ClienteId == cliente.Id);
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
