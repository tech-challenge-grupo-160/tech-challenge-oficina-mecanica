using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Validators.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.AcompanhamentoOS;

public class AcompanhamentoOSHandlersTests
{
    private readonly Mock<IOrdemDeServicoRepository> _ordemRepositoryMock;
    private readonly Mock<IOrdemServicoHistoricoRepository> _historicoRepositoryMock;
    private readonly Mock<IUsuarioAutenticadoService> _usuarioAutenticadoServiceMock;

    public AcompanhamentoOSHandlersTests()
    {
        _ordemRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _historicoRepositoryMock = new Mock<IOrdemServicoHistoricoRepository>(MockBehavior.Strict);
        _usuarioAutenticadoServiceMock = new Mock<IUsuarioAutenticadoService>(MockBehavior.Strict);
    }

    [Fact]
    public async Task ObterAcompanhamentoOS_DeveRetornarResumoQuandoDocumentoForDoClienteDaOrdem()
    {
        var ordem = OrdemDeServicoMock.Criar(
            id: 3000,
            status: StatusOrdemDeServico.EmExecucao,
            numero: "OS-20260604-3000");
        ordem.SetPrivateProperty(nameof(OrdemDeServico.CodigoAcompanhamento), "AC-TEST-3000");
        ordem.SetPrivateProperty(nameof(OrdemDeServico.Cliente), CriarCliente());

        var historicos = new[]
        {
            OrdemServicoHistorico.Registrar(
                ordem.Id,
                "1000",
                "mecanico",
                StatusOrdemDeServico.AguardandoAprovacao,
                StatusOrdemDeServico.EmExecucao,
                TipoEventoOrdemServico.OrcamentoAprovado,
                "Execucao iniciada.",
                new DateTime(2026, 6, 4, 11, 0, 0))
        };

        _ordemRepositoryMock
            .Setup(x => x.ObterPorCodigoAcompanhamentoAsync("AC-TEST-3000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _historicoRepositoryMock
            .Setup(x => x.ObterPorOrdemDeServicoAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(historicos);
        ConfigurarClienteAutenticado("47654866801");

        var handler = CriarHandler();

        var resultado = await handler.Handle(new ObterAcompanhamentoOSQuery
        {
            CodigoAcompanhamento = " ac-test-3000 "
        }, CancellationToken.None);

        resultado.Numero.Should().Be(ordem.Numero);
        resultado.Status.Should().Be(nameof(StatusOrdemDeServico.EmExecucao));
        resultado.DataUltimaAtualizacao.Should().Be(new DateTime(2026, 6, 4, 11, 0, 0));
    }

    [Fact]
    public async Task ObterAcompanhamentoOS_DeveLancarNotFoundQuandoCodigoNaoExistir()
    {
        _ordemRepositoryMock
            .Setup(x => x.ObterPorCodigoAcompanhamentoAsync("AC-INEXISTENTE", It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico?)null);

        var handler = CriarHandler();

        var acao = () => handler.Handle(new ObterAcompanhamentoOSQuery
        {
            CodigoAcompanhamento = "AC-INEXISTENTE"
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Acompanhamento nao encontrado.");
        _historicoRepositoryMock.Verify(
            x => x.ObterPorOrdemDeServicoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ObterAcompanhamentoOS_DeveLancarNotFoundQuandoDocumentoNaoForDoClienteDaOrdem()
    {
        var ordem = OrdemDeServicoMock.Criar(id: 3000);
        ordem.SetPrivateProperty(nameof(OrdemDeServico.CodigoAcompanhamento), "AC-TEST-3000");
        ordem.SetPrivateProperty(nameof(OrdemDeServico.Cliente), CriarCliente());

        _ordemRepositoryMock
            .Setup(x => x.ObterPorCodigoAcompanhamentoAsync("AC-TEST-3000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        ConfigurarClienteAutenticado("60617051000199");

        var handler = CriarHandler();

        var acao = () => handler.Handle(new ObterAcompanhamentoOSQuery
        {
            CodigoAcompanhamento = "AC-TEST-3000"
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Acompanhamento nao encontrado.");
        _historicoRepositoryMock.Verify(
            x => x.ObterPorOrdemDeServicoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void ObterAcompanhamentoOSValidator_DeveRejeitarCodigoVazio()
    {
        var validator = new ObterAcompanhamentoOSQueryValidator();

        var resultado = validator.TestValidate(new ObterAcompanhamentoOSQuery
        {
            CodigoAcompanhamento = ""
        });

        resultado.ShouldHaveValidationErrorFor(x => x.CodigoAcompanhamento)
            .WithErrorMessage("Codigo de acompanhamento e obrigatorio.");
    }

    private ObterAcompanhamentoOSQueryHandler CriarHandler()
    {
        return new ObterAcompanhamentoOSQueryHandler(
            _ordemRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _usuarioAutenticadoServiceMock.Object,
            NullLoggerFactory.Instance);
    }

    private void ConfigurarClienteAutenticado(string documento)
    {
        _usuarioAutenticadoServiceMock
            .Setup(x => x.ObterUsuarioAtual())
            .Returns(new UsuarioAutenticadoInfo
            {
                ClienteDocumento = documento
            });
    }

    private static Cliente CriarCliente()
    {
        return Cliente.Criar(
            "Vanessa Luna Duarte",
            Documento.Parse("476.548.668-01"),
            Telefone.Parse("15984608796"),
            Email.Parse("vanessa_luna_duarte@maissaude.adm.br"),
            new DateTime(2026, 6, 4, 10, 0, 0));
    }
}
