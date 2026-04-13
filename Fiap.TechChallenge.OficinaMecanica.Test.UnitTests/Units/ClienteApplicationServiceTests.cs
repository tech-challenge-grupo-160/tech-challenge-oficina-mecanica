using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks;
using FluentAssertions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units;

public class ClienteApplicationServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly ClienteApplicationService _service;

    public ClienteApplicationServiceTests()
    {
        _clienteRepositoryMock = ClienteRepositoryMockFactory.CreateStrict();
        _service = new ClienteApplicationService(_clienteRepositoryMock.Object);
    }

    [Fact]
    public async Task ObterClientePorCpfCnpj_DeveRetornarDtoQuandoClienteExistir()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Teste",
            CpfCnpj = "47654866801",
            Telefone = "11988887777",
            Email = "cliente@teste.com",
            DataCadastro = DateTime.UtcNow
        };

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await _service.ObterClientePorCpfCnpjAsync("476.548.668-01", CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be(cliente.Nome);
        resultado.CpfCnpj.Should().Be(cliente.CpfCnpj);
        _clienteRepositoryMock.Verify(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterClientePorCpfCnpj_DeveAceitarCnpj()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Empresa Teste",
            CpfCnpj = "60617051000199",
            Telefone = "11988887777",
            Email = "empresa@teste.com",
            DataCadastro = DateTime.UtcNow
        };

        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await _service.ObterClientePorCpfCnpjAsync("60.617.051/0001-99", CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.CpfCnpj.Should().Be(cliente.CpfCnpj);
        _clienteRepositoryMock.Verify(x => x.ObterPorCpfCnpjAsync(cliente.CpfCnpj, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterClientePorCpfCnpj_DeveLancarExcecaoQuandoNaoExistir()
    {
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCpfCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var acao = () => _service.ObterClientePorCpfCnpjAsync("476.548.668-01", CancellationToken.None);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
        _clienteRepositoryMock.Verify(x => x.ObterPorCpfCnpjAsync("47654866801", It.IsAny<CancellationToken>()), Times.Once);
    }
}
