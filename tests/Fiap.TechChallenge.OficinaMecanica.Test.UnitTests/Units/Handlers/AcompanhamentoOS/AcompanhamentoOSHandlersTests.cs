using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Validators.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
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

    public AcompanhamentoOSHandlersTests()
    {
        _ordemRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _historicoRepositoryMock = new Mock<IOrdemServicoHistoricoRepository>(MockBehavior.Strict);
    }

    [Fact]
    public async Task ObterAcompanhamentoOS_DeveRetornarResumoQuandoCodigoETokenForemValidos()
    {
        var token = "token-publico";
        var ordem = OrdemDeServicoMock.Criar(
            id: 3000,
            status: StatusOrdemDeServico.EmExecucao,
            numero: "OS-20260604-3000");
        ordem.SetPrivateProperty(nameof(OrdemDeServico.CodigoAcompanhamento), "AC-TEST-3000");
        ordem.SetPrivateProperty(nameof(OrdemDeServico.TokenAcompanhamentoHash), StringHelper.ToSha256Hash(token));

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

        var handler = CriarHandler();

        var resultado = await handler.Handle(new ObterAcompanhamentoOSQuery
        {
            Codigo = " ac-test-3000 ",
            Token = $" {token} "
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
            Codigo = "AC-INEXISTENTE",
            Token = "token"
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Acompanhamento nao encontrado.");
        _historicoRepositoryMock.Verify(
            x => x.ObterPorOrdemDeServicoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ObterAcompanhamentoOS_DeveLancarNotFoundQuandoTokenForInvalido()
    {
        var ordem = OrdemDeServicoMock.Criar(id: 3000);
        ordem.SetPrivateProperty(nameof(OrdemDeServico.CodigoAcompanhamento), "AC-TEST-3000");
        ordem.SetPrivateProperty(nameof(OrdemDeServico.TokenAcompanhamentoHash), StringHelper.ToSha256Hash("token-correto"));

        _ordemRepositoryMock
            .Setup(x => x.ObterPorCodigoAcompanhamentoAsync("AC-TEST-3000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var handler = CriarHandler();

        var acao = () => handler.Handle(new ObterAcompanhamentoOSQuery
        {
            Codigo = "AC-TEST-3000",
            Token = "token-incorreto"
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Acompanhamento nao encontrado.");
        _historicoRepositoryMock.Verify(
            x => x.ObterPorOrdemDeServicoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void ObterAcompanhamentoOSValidator_DeveRejeitarCodigoETokenVazios()
    {
        var validator = new ObterAcompanhamentoOSQueryValidator();

        var resultado = validator.TestValidate(new ObterAcompanhamentoOSQuery
        {
            Codigo = "",
            Token = ""
        });

        resultado.ShouldHaveValidationErrorFor(x => x.Codigo)
            .WithErrorMessage("Codigo de acompanhamento e obrigatorio.");
        resultado.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Token de acompanhamento e obrigatorio.");
    }

    private ObterAcompanhamentoOSQueryHandler CriarHandler()
    {
        return new ObterAcompanhamentoOSQueryHandler(
            _ordemRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            NullLoggerFactory.Instance);
    }
}
