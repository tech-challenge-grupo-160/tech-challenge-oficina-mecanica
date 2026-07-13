using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Commands;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.Clientes;

public class ClienteHandlersTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock;
    private readonly Mock<IOrdemDeServicoRepository> _ordemDeServicoRepositoryMock;
    private readonly Mock<IClock> _clockMock;

    public ClienteHandlersTests()
    {
        _clienteRepositoryMock = ClienteRepositoryMockFactory.CreateStrict();
        _veiculoRepositoryMock = new Mock<IVeiculoRepository>(MockBehavior.Strict);
        _ordemDeServicoRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _clockMock = new Mock<IClock>(MockBehavior.Strict);
        _clockMock.Setup(x => x.Now).Returns(new DateTime(2026, 6, 4, 10, 0, 0));
    }

    [Fact]
    public async Task ObterClientePorId_DeveRetornarResultQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(id: 1, cpfCnpj: "47654866801", nome: "Cliente Teste");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var handler = new ObterClientePorIdQueryHandler(_clienteRepositoryMock.Object);

        var resultado = await handler.Handle(
            new ObterClientePorIdQuery { Id = cliente.Id },
            CancellationToken.None);

        resultado.Id.Should().Be(cliente.Id);
        resultado.Nome.Should().Be(cliente.Nome);
        resultado.CpfCnpj.Should().Be(cliente.CpfCnpj.Valor);
    }

    [Fact]
    public async Task ObterClientePorDocumento_DeveRetornarResultQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(id: 1, cpfCnpj: "47654866801", nome: "Cliente Teste");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var handler = new ObterClientePorDocumentoQueryHandler(
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(
            new ObterClientePorDocumentoQuery { CpfCnpj = "476.548.668-01" },
            CancellationToken.None);

        resultado.Nome.Should().Be(cliente.Nome);
        resultado.CpfCnpj.Should().Be(cliente.CpfCnpj.Valor);
    }

    [Fact]
    public async Task ObterClientePorDocumento_DeveAceitarCnpj()
    {
        var cliente = ClienteMock.Criar(id: 2, cpfCnpj: "60617051000199", nome: "Empresa Teste");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var handler = new ObterClientePorDocumentoQueryHandler(
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(
            new ObterClientePorDocumentoQuery { CpfCnpj = "60.617.051/0001-99" },
            CancellationToken.None);

        resultado.CpfCnpj.Should().Be(cliente.CpfCnpj.Valor);
    }

    [Fact]
    public async Task ObterClientePorDocumento_DeveLancarExcecaoQuandoNaoExistir()
    {
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var handler = new ObterClientePorDocumentoQueryHandler(
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(
            new ObterClientePorDocumentoQuery { CpfCnpj = "476.548.668-01" },
            CancellationToken.None);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CriarCliente_DevePersistirQuandoDocumentoNaoExiste()
    {
        var command = CriarClienteCommandMock.Criar(
            nome: "Novo Cliente",
            cpfCnpj: "529.982.247-25",
            email: "novo@cliente.com",
            telefone: "11999999999");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("52998224725"), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        _clienteRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente cliente, CancellationToken _) => cliente);

        var handler = new CriarClienteCommandHandler(
            _clienteRepositoryMock.Object,
            _clockMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(command, CancellationToken.None);

        resultado.Nome.Should().Be(command.Nome);
        resultado.CpfCnpj.Should().Be("52998224725");
    }

    [Fact]
    public async Task CriarCliente_DeveLancarQuandoDocumentoJaExistir()
    {
        var command = CriarClienteCommandMock.Criar(
            nome: "Cliente Existente",
            cpfCnpj: "476.548.668-01",
            email: "cliente@teste.com",
            telefone: "11988887777");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(1, "47654866801", "Cliente Existente"));

        var handler = new CriarClienteCommandHandler(
            _clienteRepositoryMock.Object,
            _clockMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(command, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AtualizarCliente_DeveAtualizarQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");
        var command = new AtualizarClienteCommand
        {
            CpfCnpj = "476.548.668-01",
            Nome = "Vanessa Atualizada",
            Email = "vanessa@teste.com",
            Telefone = "11987654321"
        };

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken _) => c);

        var handler = new AtualizarClienteCommandHandler(
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(command, CancellationToken.None);

        resultado.Nome.Should().Be("Vanessa Atualizada");
        resultado.Telefone.Should().Be("11987654321");
        resultado.Email.Should().Be("vanessa@teste.com");
    }

    [Fact]
    public async Task DeletarCliente_DeveLancarQuandoExistiremVeiculosVinculados()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeletarClienteCommandHandler(
            _clienteRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _ordemDeServicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(
            new DeletarClienteCommand { CpfCnpj = "476.548.668-01" },
            CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*veiculos vinculados*");
    }

    [Fact]
    public async Task DeletarCliente_DeveLancarQuandoExistiremOrdensDeServicoVinculadas()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ordemDeServicoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeletarClienteCommandHandler(
            _clienteRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _ordemDeServicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var acao = () => handler.Handle(
            new DeletarClienteCommand { CpfCnpj = "476.548.668-01" },
            CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ordens de servico vinculadas*");
    }

    [Fact]
    public async Task DeletarCliente_DeveExcluirQuandoNaoExistiremDependencias()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(Documento.Parse("47654866801"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ordemDeServicoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _clienteRepositoryMock
            .Setup(x => x.DeletarAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeletarClienteCommandHandler(
            _clienteRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _ordemDeServicoRepositoryMock.Object,
            NullLoggerFactory.Instance);

        await handler.Handle(new DeletarClienteCommand { CpfCnpj = "476.548.668-01" }, CancellationToken.None);

        _clienteRepositoryMock.Verify(x => x.DeletarAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarClientes_DeveRetornarPaginadoComFiltroPorNomeEDocumento()
    {
        var clientes = new[]
        {
            ClienteMock.Criar(1, "47654866801", "Vanessa Luna Duarte")
        };

        _clienteRepositoryMock
            .Setup(x => x.ContarAsync("Vanessa", "476", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _clienteRepositoryMock
            .Setup(x => x.ObterPaginadoAsync(1, 10, "Vanessa", "476", It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var handler = new ListarClientesQueryHandler(
            _clienteRepositoryMock.Object,
            NullLoggerFactory.Instance);

        var resultado = await handler.Handle(
            new ListarClientesQuery
            {
                Page = 1,
                PageSize = 10,
                Nome = " Vanessa ",
                CpfCnpj = "476"
            },
            CancellationToken.None);

        resultado.Items.Should().HaveCount(1);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(10);
        resultado.TotalItems.Should().Be(1);
        resultado.TotalPages.Should().Be(1);
    }
}
