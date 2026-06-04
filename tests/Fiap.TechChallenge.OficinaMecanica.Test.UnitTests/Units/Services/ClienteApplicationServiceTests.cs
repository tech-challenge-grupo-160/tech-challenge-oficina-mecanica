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

public class ClienteApplicationServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock;
    private readonly Mock<IOrdemDeServicoRepository> _ordemDeServicoRepositoryMock;
    private readonly ClienteApplicationService _service;

    public ClienteApplicationServiceTests()
    {
        _clienteRepositoryMock = ClienteRepositoryMockFactory.CreateStrict();
        _veiculoRepositoryMock = new Mock<IVeiculoRepository>(MockBehavior.Strict);
        _ordemDeServicoRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _service = new ClienteApplicationService(
            _clienteRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _ordemDeServicoRepositoryMock.Object,
            NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task ObterClientePorCpfCnpj_DeveRetornarDtoQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(id: 1, cpfCnpj: "47654866801", nome: "Cliente Teste");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await _service.ObterClientePorCpfCnpjAsync("476.548.668-01", CancellationToken.None);

        resultado.Nome.Should().Be(cliente.Nome);
        resultado.CpfCnpj.Should().Be(cliente.CpfCnpj);
    }

    [Fact]
    public async Task ObterClientePorCpfCnpj_DeveAceitarCnpj()
    {
        var cliente = ClienteMock.Criar(id: 2, cpfCnpj: "60617051000199", nome: "Empresa Teste");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await _service.ObterClientePorCpfCnpjAsync("60.617.051/0001-99", CancellationToken.None);

        resultado.CpfCnpj.Should().Be(cliente.CpfCnpj);
    }

    [Fact]
    public async Task ObterClientePorCpfCnpj_DeveLancarExcecaoQuandoNaoExistir()
    {
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var acao = () => _service.ObterClientePorCpfCnpjAsync("476.548.668-01", CancellationToken.None);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CriarCliente_DevePersistirQuandoDocumentoNaoExiste()
    {
        var dto = CriarClienteDtoMock.Criar(
            nome: "Novo Cliente",
            cpfCnpj: "529.982.247-25",
            email: "novo@cliente.com",
            telefone: "11999999999");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("52998224725", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        _clienteRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente cliente, CancellationToken _) => cliente);

        var resultado = await _service.CriarClienteAsync(dto, CancellationToken.None);

        resultado.Nome.Should().Be(dto.Nome);
        resultado.CpfCnpj.Should().Be("52998224725");
    }

    [Fact]
    public async Task CriarCliente_DeveLancarQuandoDocumentoJaExistir()
    {
        var dto = CriarClienteDtoMock.Criar(
            nome: "Cliente Existente",
            cpfCnpj: "476.548.668-01",
            email: "cliente@teste.com",
            telefone: "11988887777");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClienteMock.Criar(1, "47654866801", "Cliente Existente"));

        var acao = () => _service.CriarClienteAsync(dto, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AtualizarClientePorCpfCnpj_DeveAtualizarQuandoClienteExistir()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");
        var dto = new AtualizarClienteDto
        {
            Nome = "Vanessa Atualizada",
            Email = "vanessa@teste.com",
            Telefone = "11987654321"
        };

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken _) => c);

        var resultado = await _service.AtualizarClientePorCpfCnpjAsync("476.548.668-01", dto, CancellationToken.None);

        resultado.Nome.Should().Be("Vanessa Atualizada");
        resultado.Telefone.Should().Be("11987654321");
        resultado.Email.Should().Be("vanessa@teste.com");
    }

    [Fact]
    public async Task DeletarClientePorCpfCnpj_DeveLancarQuandoExistiremVeiculosVinculados()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var acao = () => _service.DeletarClientePorCpfCnpjAsync("476.548.668-01", CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*veiculos vinculados*");
    }

    [Fact]
    public async Task DeletarClientePorCpfCnpj_DeveLancarQuandoExistiremOrdensDeServicoVinculadas()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ordemDeServicoRepositoryMock
            .Setup(x => x.ExistePorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var acao = () => _service.DeletarClientePorCpfCnpjAsync("476.548.668-01", CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ordens de servico vinculadas*");
    }

    [Fact]
    public async Task DeletarClientePorCpfCnpj_DeveExcluirQuandoNaoExistiremDependencias()
    {
        var cliente = ClienteMock.Criar(1, "47654866801", "Vanessa");

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()))
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

        await _service.DeletarClientePorCpfCnpjAsync("476.548.668-01", CancellationToken.None);

        _clienteRepositoryMock.Verify(x => x.DeletarAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarClientesAsync_DeveRetornarPaginadoComFiltroPorNomeEDocumento()
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

        var resultado = await _service.ListarClientesAsync(1, 10, " Vanessa ", "476", CancellationToken.None);

        resultado.Items.Should().HaveCount(1);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(10);
        resultado.TotalItems.Should().Be(1);
        resultado.TotalPages.Should().Be(1);
    }
}
