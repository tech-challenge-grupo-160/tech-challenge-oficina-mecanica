using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Clientes;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Clientes;

public class ClientesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ClientesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ObterPorDocumento_DeveRetornarClienteExistente()
    {
        var response = await _client.GetAsync("/api/v1/Clientes/documento/47654866801");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        cliente.Should().NotBeNull();
        cliente!.Nome.Should().Be("Vanessa Luna Duarte");
        cliente.CpfCnpj.Should().Be("47654866801");
    }

    [Fact]
    public async Task ObterPorDocumento_DeveRetornarEmpresaQuandoCnpj()
    {
        var response = await _client.GetAsync("/api/v1/Clientes/documento/60617051000199");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        cliente.Should().NotBeNull();
        cliente!.Nome.Should().Be("Betina e Fernanda Contabil Ltda");
        cliente.CpfCnpj.Should().Be("60617051000199");
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarCreatedQuandoPayloadValido()
    {
        var payload = new
        {
            nome = "Cliente Integracao",
            cpfCnpj = "52998224725",
            email = "integracao@teste.com",
            telefone = "11988887777"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Clientes", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        cliente.Should().NotBeNull();
        cliente!.Nome.Should().Be("Cliente Integracao");
        cliente.CpfCnpj.Should().Be("52998224725");
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarBadRequestQuandoDadosObrigatoriosFaltarem()
    {
        var payload = new
        {
            cpfCnpj = "59362967063",
            telefone = "11987654321"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Clientes", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListarClientes_DeveRetornarPaginado()
    {
        var response = await _client.GetAsync("/api/v1/Clientes?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultado = await response.Content.ReadFromJsonAsync<PagedResponse<ClienteResponse>>();
        resultado.Should().NotBeNull();
        resultado!.Items.Count.Should().BeGreaterThanOrEqualTo(2);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ListarClientes_DeveFiltrarPorNome()
    {
        var response = await _client.GetAsync("/api/v1/Clientes?nome=vanessa&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultado = await response.Content.ReadFromJsonAsync<PagedResponse<ClienteResponse>>();
        resultado.Should().NotBeNull();
        resultado!.Items.Should().ContainSingle();
        resultado.Items.Single().Nome.Should().Be("Vanessa Luna Duarte");
    }

    [Fact]
    public async Task ListarClientes_DeveFiltrarPorDocumentoParcial()
    {
        var response = await _client.GetAsync("/api/v1/Clientes?cpfCnpj=60617051&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultado = await response.Content.ReadFromJsonAsync<PagedResponse<ClienteResponse>>();
        resultado.Should().NotBeNull();
        resultado!.Items.Should().ContainSingle();
        resultado.Items.Single().CpfCnpj.Should().Be("60617051000199");
    }

    [Fact]
    public async Task AtualizarPorDocumento_DeveRetornarClienteAtualizado()
    {
        var cpf = GerarCpfValido();
        var createPayload = new
        {
            nome = "Cliente Atualizacao",
            cpfCnpj = cpf,
            email = "cliente.atualizacao@teste.com",
            telefone = "11911112222"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/Clientes", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = new
        {
            nome = "Cliente Atualizado",
            telefone = "11976543210",
            email = "cliente.atualizado@teste.com"
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/Clientes/documento/{cpf}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        cliente.Should().NotBeNull();
        cliente!.Nome.Should().Be("Cliente Atualizado");
        cliente.Telefone.Should().Be("11976543210");
        cliente.Email.Should().Be("cliente.atualizado@teste.com");
    }

    [Fact]
    public async Task DeletarPorDocumento_DeveRetornarBadRequestQuandoClientePossuirVeiculo()
    {
        var response = await _client.DeleteAsync("/api/v1/Clientes/documento/47654866801");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("veiculos vinculados");
    }

    [Fact]
    public async Task ListarVeiculosPorDocumento_DeveRetornarVeiculosDoCliente()
    {
        var response = await _client.GetAsync("/api/v1/Clientes/47654866801/veiculos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var veiculos = await response.Content.ReadFromJsonAsync<List<VeiculoResponse>>();
        veiculos.Should().NotBeNull();
        veiculos!.Should().ContainSingle();
        veiculos.Single().Placa.Should().Be("BRA2E19");
    }

    [Fact]
    public async Task CriarVeiculoParaCliente_DeveRetornarCreated()
    {
        var payload = new
        {
            placa = "abc1d23",
            marca = "Toyota",
            modelo = "Corolla",
            ano = 2021
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Clientes/60617051000199/veiculos", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var veiculo = await response.Content.ReadFromJsonAsync<VeiculoResponse>();
        veiculo.Should().NotBeNull();
        veiculo!.Placa.Should().Be("ABC1D23");
        veiculo.ClienteId.Should().Be(CustomWebApplicationFactory.PessoaJuridicaClienteId);
    }

    [Fact]
    public async Task DeletarPorDocumento_DeveRetornarNoContentQuandoClienteNaoPossuirDependencias()
    {
        var cpf = GerarCpfValido();
        var payload = new
        {
            nome = "Cliente Sem Vinculos",
            cpfCnpj = cpf,
            email = "sem.vinculos@teste.com",
            telefone = "11999990000"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/Clientes", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/Clientes/documento/{cpf}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static string GerarCpfValido()
    {
        var random = Guid.NewGuid().ToString("N");
        var baseDigits = new int[9];

        for (var i = 0; i < baseDigits.Length; i++)
        {
            baseDigits[i] = random[i] % 10;
        }

        var digito1 = CalcularDigitoCpf(baseDigits, 10);
        var digito2 = CalcularDigitoCpf(baseDigits.Append(digito1).ToArray(), 11);

        return string.Concat(baseDigits) + digito1 + digito2;
    }

    private static int CalcularDigitoCpf(int[] digits, int pesoInicial)
    {
        var soma = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            soma += digits[i] * (pesoInicial - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
