using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Services;

public class OrdemDeServicoApplicationServiceTests
{
    private readonly Mock<IOrdemDeServicoRepository> _ordemRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock;
    private readonly Mock<IServicoRepository> _servicoRepositoryMock;
    private readonly Mock<IPecaRepository> _pecaRepositoryMock;
    private readonly Mock<IPedidoCompraRepository> _pedidoCompraRepositoryMock;
    private readonly Mock<IMovimentacaoEstoqueRepository> _movimentacaoEstoqueRepositoryMock;
    private readonly Mock<IOrdemServicoHistoricoRepository> _historicoRepositoryMock;
    private readonly Mock<INotificacaoClienteRepository> _notificacaoClienteRepositoryMock;
    private readonly Mock<IUsuarioAutenticadoService> _usuarioAutenticadoServiceMock;
    private readonly Mock<ITransactionManager> _transactionManagerMock;
    private readonly OrdemDeServicoApplicationService _service;

    public OrdemDeServicoApplicationServiceTests()
    {
        _ordemRepositoryMock = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        _clienteRepositoryMock = new Mock<IClienteRepository>(MockBehavior.Strict);
        _veiculoRepositoryMock = new Mock<IVeiculoRepository>(MockBehavior.Strict);
        _servicoRepositoryMock = new Mock<IServicoRepository>(MockBehavior.Strict);
        _pecaRepositoryMock = new Mock<IPecaRepository>(MockBehavior.Strict);
        _pedidoCompraRepositoryMock = new Mock<IPedidoCompraRepository>(MockBehavior.Strict);
        _movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>(MockBehavior.Strict);
        _historicoRepositoryMock = new Mock<IOrdemServicoHistoricoRepository>(MockBehavior.Strict);
        _notificacaoClienteRepositoryMock = new Mock<INotificacaoClienteRepository>(MockBehavior.Strict);
        _usuarioAutenticadoServiceMock = new Mock<IUsuarioAutenticadoService>(MockBehavior.Strict);
        _transactionManagerMock = new Mock<ITransactionManager>(MockBehavior.Strict);

        _usuarioAutenticadoServiceMock
            .Setup(x => x.ObterUsuarioAtual())
            .Returns(new UsuarioAutenticadoInfo
            {
                UsuarioId = "1000",
                UsuarioNome = "unit-test-user"
            });
        _historicoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoHistorico historico, CancellationToken _) => historico);
        _notificacaoClienteRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<NotificacaoCliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificacaoCliente notificacao, CancellationToken _) => notificacao);
        _transactionManagerMock
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<OrdemDeServico>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<OrdemDeServico>> action, CancellationToken ct) => action(ct));
        _ordemRepositoryMock
            .Setup(x => x.ObterPorCodigoAcompanhamentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico?)null);

        _service = new OrdemDeServicoApplicationService(
            _ordemRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _servicoRepositoryMock.Object,
            _pecaRepositoryMock.Object,
            _pedidoCompraRepositoryMock.Object,
            _movimentacaoEstoqueRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _notificacaoClienteRepositoryMock.Object,
            _usuarioAutenticadoServiceMock.Object,
            _transactionManagerMock.Object,
            NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task CriarOrdemDeServicoAsync_DeveAbrirOsEmRecebida()
    {
        var dto = CriarOrdemDeServicoDtoMock.Criar(clienteId: 1, veiculoId: 10);
        var cliente = ClienteMock.Criar(id: 1);
        var veiculo = VeiculoMock.Criar(id: 10, clienteId: 1);

        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(dto.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(dto.VeiculoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);
        _ordemRepositoryMock
            .Setup(x => x.ExisteOrdemAtivaPorClienteEVeiculoAsync(dto.ClienteId, dto.VeiculoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ordemRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico ordem, CancellationToken _) =>
            {
                ordem.Id = 3002;
                return ordem;
            });
        _ordemRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico ordem, CancellationToken _) => ordem);

        var resultado = await _service.CriarOrdemDeServicoAsync(dto, CancellationToken.None);

        resultado.Status.Should().Be(nameof(StatusOrdemDeServico.Recebida));
        resultado.Numero.Should().Be($"OS-{resultado.DataAbertura:yyyyMMdd}-3002");
        resultado.ValorTotal.Should().Be(0);
        _historicoRepositoryMock.Verify(
            x => x.CriarAsync(
                It.Is<OrdemServicoHistorico>(h =>
                    h.OrdemDeServicoId == 3002 &&
                    h.UsuarioId == "1000" &&
                    h.UsuarioNome == "unit-test-user" &&
                    h.StatusAnterior == null &&
                    h.StatusNovo == StatusOrdemDeServico.Recebida &&
                    h.TipoEvento == TipoEventoOrdemServico.OrdemCriada),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CriarOrdemDeServicoAsync_DeveLancarQuandoJaExistirOsAtivaParaMesmoClienteEVeiculo()
    {
        var dto = CriarOrdemDeServicoDtoMock.Criar(clienteId: 1, veiculoId: 10);
        var cliente = ClienteMock.Criar(id: 1);
        var veiculo = VeiculoMock.Criar(id: 10, clienteId: 1);

        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(dto.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _veiculoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(dto.VeiculoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);
        _ordemRepositoryMock
            .Setup(x => x.ExisteOrdemAtivaPorClienteEVeiculoAsync(dto.ClienteId, dto.VeiculoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var acao = () => _service.CriarOrdemDeServicoAsync(dto, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceValidationException>()
            .WithMessage("*ordem de servico ativa*");
    }

    [Fact]
    public async Task FinalizarDiagnosticoAsync_DeveLancarQuandoNaoHouverServico()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.FinalizarDiagnosticoAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ao menos um servico*");
    }

    [Fact]
    public async Task FinalizarDiagnosticoAsync_DeveLancarQuandoOrcamentoForZero()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico);
        ordem.Servicos.Add(new OrdemDeServicoServico
        {
            OrdemDeServicoId = ordem.Id,
            ServicoId = 1000,
            Preco = 0,
            TempoEstimado = 30
        });

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.FinalizarDiagnosticoAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nao pode ser zerado*");
    }

    [Fact]
    public async Task FluxoCompleto_DeveAtualizarStatusEDatasCorretamente()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.Recebida, clienteId: 1, veiculoId: 10);
        var servico = ServicoMock.Criar(id: 1000, preco: 150m);
        var peca = PecaMock.Criar(id: 1000, preco: 45m, quantidadeEstoque: 5);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _ordemRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico os, CancellationToken _) => os);
        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pecaRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Peca>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Peca item, CancellationToken _) => item);
        _movimentacaoEstoqueRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovimentacaoEstoque movimentacao, CancellationToken _) => movimentacao);

        var emDiagnostico = await _service.IniciarDiagnosticoAsync(ordem.Id, CancellationToken.None);
        var comServico = await _service.AdicionarServicoAsync(ordem.Id, new AdicionarServicoAOrdemDto { ServicoId = servico.Id }, CancellationToken.None);
        var comPeca = await _service.AdicionarPecaAsync(ordem.Id, new AdicionarPecaAOrdemDto { PecaId = peca.Id, Quantidade = 1 }, CancellationToken.None);
        var aguardandoAprovacao = await _service.FinalizarDiagnosticoAsync(ordem.Id, CancellationToken.None);
        var emExecucao = await _service.AprovarAsync(ordem.Id, CancellationToken.None);
        var finalizada = await _service.FinalizarAsync(ordem.Id, CancellationToken.None);
        var pagamentoRegistrado = await _service.RegistrarPagamentoAsync(ordem.Id, CancellationToken.None);
        var entregue = await _service.EntregarAsync(ordem.Id, CancellationToken.None);

        emDiagnostico.Status.Should().Be(nameof(StatusOrdemDeServico.EmDiagnostico));
        comServico.ValorTotal.Should().Be(150m);
        comPeca.ValorTotal.Should().Be(195m);
        peca.QuantidadeEstoque.Should().Be(4);
        aguardandoAprovacao.Status.Should().Be(nameof(StatusOrdemDeServico.AguardandoAprovacao));
        aguardandoAprovacao.OrcamentoEnviadoEm.Should().NotBeNull();
        emExecucao.Status.Should().Be(nameof(StatusOrdemDeServico.EmExecucao));
        finalizada.Status.Should().Be(nameof(StatusOrdemDeServico.Finalizada));
        finalizada.DataFinalizacao.Should().NotBeNull();
        pagamentoRegistrado.DataPagamento.Should().NotBeNull();
        entregue.Status.Should().Be(nameof(StatusOrdemDeServico.Entregue));
        entregue.DataConclusao.Should().NotBeNull();
        _historicoRepositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<OrdemServicoHistorico>(), It.IsAny<CancellationToken>()),
            Times.Exactly(9));
        _notificacaoClienteRepositoryMock.Verify(
            x => x.CriarAsync(
                It.Is<NotificacaoCliente>(n =>
                    n.OrdemDeServicoId == ordem.Id &&
                    n.TipoNotificacao == TipoNotificacaoCliente.OrcamentoDisponivel &&
                    n.Canal == CanalNotificacaoCliente.WhatsApp &&
                    n.Recebida),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _notificacaoClienteRepositoryMock.Verify(
            x => x.CriarAsync(
                It.Is<NotificacaoCliente>(n =>
                    n.OrdemDeServicoId == ordem.Id &&
                    n.TipoNotificacao == TipoNotificacaoCliente.ServicoFinalizado &&
                    n.Canal == CanalNotificacaoCliente.WhatsApp &&
                    n.Recebida),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EntregarAsync_DeveLancarQuandoPagamentoNaoTiverSidoRegistrado()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.Finalizada);
        ordem.DataFinalizacao = DateTime.UtcNow;

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.EntregarAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*apos o pagamento*");
    }

    [Fact]
    public async Task ObterMonitoramentoAsync_DeveRetornarTempoDeFinalizacaoQuandoOsEstiverFechada()
    {
        var dataAbertura = new DateTime(2026, 4, 20, 8, 0, 0);
        var dataFinalizacao = dataAbertura.AddHours(5).AddMinutes(30);
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.Finalizada);
        ordem.DataAbertura = dataAbertura;
        ordem.DataFinalizacao = dataFinalizacao;

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var resultado = await _service.ObterMonitoramentoAsync(ordem.Id, CancellationToken.None);

        resultado.EstaFinalizada.Should().BeTrue();
        resultado.DataFinalizacao.Should().Be(dataFinalizacao);
        resultado.TempoFinalizacaoMinutos.Should().Be(330);
        resultado.TempoFinalizacaoHoras.Should().Be(5.5);
        resultado.TempoDecorridoMinutos.Should().Be(330);
    }

    [Fact]
    public async Task ObterResumoMonitoramentoAsync_DeveCalcularMediaDeFinalizacao()
    {
        var ordemFinalizadaRapida = OrdemDeServicoMock.Criar(id: 1, status: StatusOrdemDeServico.Finalizada, numero: "OS-1");
        ordemFinalizadaRapida.DataAbertura = new DateTime(2026, 4, 20, 8, 0, 0);
        ordemFinalizadaRapida.DataFinalizacao = ordemFinalizadaRapida.DataAbertura.AddHours(2);

        var ordemFinalizadaLenta = OrdemDeServicoMock.Criar(id: 2, status: StatusOrdemDeServico.Finalizada, numero: "OS-2");
        ordemFinalizadaLenta.DataAbertura = new DateTime(2026, 4, 20, 9, 0, 0);
        ordemFinalizadaLenta.DataFinalizacao = ordemFinalizadaLenta.DataAbertura.AddHours(4);

        var ordemAberta = OrdemDeServicoMock.Criar(id: 3, status: StatusOrdemDeServico.EmExecucao, numero: "OS-3");
        ordemAberta.DataAbertura = new DateTime(2026, 4, 20, 10, 0, 0);
        ordemAberta.DataFinalizacao = null;

        _ordemRepositoryMock
            .Setup(x => x.ObterTodasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ordemFinalizadaRapida, ordemFinalizadaLenta, ordemAberta });

        var resultado = await _service.ObterResumoMonitoramentoAsync(1, 2, CancellationToken.None);

        resultado.TotalOrdens.Should().Be(3);
        resultado.TotalOrdensAbertas.Should().Be(1);
        resultado.TotalOrdensFinalizadas.Should().Be(2);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(2);
        resultado.TotalPages.Should().Be(2);
        resultado.TempoMedioFinalizacaoMinutos.Should().Be(180);
        resultado.TempoMedioFinalizacaoHoras.Should().Be(3);
        resultado.Ordens.Should().HaveCount(2);
        resultado.Ordens.Should().Contain(x => x.Id == 1 && x.TempoFinalizacaoMinutos == 120);
        resultado.Ordens.Should().Contain(x => x.Id == 2 && x.TempoFinalizacaoMinutos == 240);
    }

    [Fact]
    public async Task ObterEstimativaTempoAsync_DeveSomarTempoEstimadoDosServicosDaOs()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico, clienteId: 1, veiculoId: 10);
        ordem.Servicos.Add(new OrdemDeServicoServico
        {
            OrdemDeServicoId = ordem.Id,
            ServicoId = 1000,
            Preco = 150m,
            TempoEstimado = 30
        });
        ordem.Servicos.Add(new OrdemDeServicoServico
        {
            OrdemDeServicoId = ordem.Id,
            ServicoId = 1001,
            Preco = 300m,
            TempoEstimado = 90
        });

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var resultado = await _service.ObterEstimativaTempoAsync(ordem.Id, CancellationToken.None);

        resultado.OrdemDeServicoId.Should().Be(ordem.Id);
        resultado.TotalServicos.Should().Be(2);
        resultado.TempoEstimadoMinutos.Should().Be(120);
        resultado.TempoEstimadoHoras.Should().Be(2);
        resultado.Servicos.Should().Contain(x => x.ServicoId == 1000 && x.TempoEstimadoMinutos == 30 && x.TempoEstimadoHoras == 0.5);
        resultado.Servicos.Should().Contain(x => x.ServicoId == 1001 && x.TempoEstimadoMinutos == 90 && x.TempoEstimadoHoras == 1.5);
    }

    [Fact]
    public async Task AprovarAsync_DeveBloquearExecucaoEGerarPedidoCompraQuandoFaltarEstoque()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoAprovacao, clienteId: 1, veiculoId: 10);
        ordem.Pecas.Add(new OrdemDeServicoPeca
        {
            OrdemDeServicoId = ordem.Id,
            PecaId = 1000,
            Quantidade = 3,
            Preco = 45m
        });

        var peca = PecaMock.Criar(id: 1000, quantidadeEstoque: 1);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _ordemRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico item, CancellationToken _) => item);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pedidoCompraRepositoryMock
            .Setup(x => x.ObterPendentePorOrdemEPecaAsync(ordem.Id, peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoCompra?)null);
        _pedidoCompraRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<PedidoCompra>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoCompra pedido, CancellationToken _) =>
            {
                pedido.Id = 50;
                return pedido;
            });

        var resultado = await _service.AprovarAsync(ordem.Id, CancellationToken.None);

        resultado.Status.Should().Be(nameof(StatusOrdemDeServico.AguardandoEstoque));
        _pedidoCompraRepositoryMock.Verify(x => x.CriarAsync(It.Is<PedidoCompra>(p =>
            p.OrdemDeServicoId == ordem.Id &&
            p.PecaId == peca.Id &&
            p.QuantidadeSolicitada == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AprovarAsync_DeveLancarQuandoOsEstiverAguardandoEstoque()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoEstoque, clienteId: 1, veiculoId: 10);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.AprovarAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*aguardando estoque*");
    }

    [Fact]
    public async Task LiberarExecucaoAsync_DeveBaixarEstoqueEMudarParaEmExecucaoQuandoHouverDisponibilidade()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoEstoque, clienteId: 1, veiculoId: 10);
        ordem.Pecas.Add(new OrdemDeServicoPeca
        {
            OrdemDeServicoId = ordem.Id,
            PecaId = 1000,
            Quantidade = 3,
            Preco = 45m
        });

        var peca = PecaMock.Criar(id: 1000, quantidadeEstoque: 3);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);
        _pecaRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Peca>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Peca item, CancellationToken _) => item);
        _movimentacaoEstoqueRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovimentacaoEstoque item, CancellationToken _) => item);
        _ordemRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico item, CancellationToken _) => item);

        var resultado = await _service.LiberarExecucaoAsync(ordem.Id, CancellationToken.None);

        resultado.Status.Should().Be(nameof(StatusOrdemDeServico.EmExecucao));
        peca.QuantidadeEstoque.Should().Be(0);
        _movimentacaoEstoqueRepositoryMock.Verify(x => x.CriarAsync(It.Is<MovimentacaoEstoque>(m =>
            m.OrdemDeServicoId == ordem.Id &&
            m.PecaId == peca.Id &&
            m.Quantidade == 3 &&
            m.QuantidadeAnterior == 3 &&
            m.QuantidadePosterior == 0), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LiberarExecucaoAsync_DeveLancarQuandoEstoqueContinuarIndisponivel()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoEstoque, clienteId: 1, veiculoId: 10);
        ordem.Pecas.Add(new OrdemDeServicoPeca
        {
            OrdemDeServicoId = ordem.Id,
            PecaId = 1000,
            Quantidade = 3,
            Preco = 45m
        });

        var peca = PecaMock.Criar(id: 1000, quantidadeEstoque: 1);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(peca.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(peca);

        var acao = () => _service.LiberarExecucaoAsync(ordem.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Estoque indisponivel*");
        ordem.Status.Should().Be(StatusOrdemDeServico.AguardandoEstoque);
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveLancarQuandoTentativaDiretaParaEmExecucao()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoEstoque, clienteId: 1, veiculoId: 10);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var acao = () => _service.AtualizarStatusAsync(
            ordem.Id,
            new AtualizarStatusOrdemDeServicoDto { NovoStatus = nameof(StatusOrdemDeServico.EmExecucao) },
            CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*diretamente para EmExecucao*");
        ordem.Status.Should().Be(StatusOrdemDeServico.AguardandoEstoque);
        _ordemRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoverServicoAsync_DeveRemoverServicoQuandoOsEstiverEmDiagnostico()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico, clienteId: 1, veiculoId: 10);
        ordem.Servicos.Add(new OrdemDeServicoServico
        {
            OrdemDeServicoId = ordem.Id,
            ServicoId = 1000,
            Preco = 150m,
            TempoEstimado = 30
        });
        ordem.ValorTotal = 150m;
        var servico = ServicoMock.Criar(id: 1000, preco: 150m);

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);
        _ordemRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServico item, CancellationToken _) => item);

        var resultado = await _service.RemoverServicoAsync(ordem.Id, servico.Id, CancellationToken.None);

        resultado.Servicos.Should().BeEmpty();
        resultado.ValorTotal.Should().Be(0);
    }

    [Fact]
    public async Task RemoverPecaAsync_DeveLancarQuandoOsNaoEstiverEmDiagnostico()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.AguardandoAprovacao, clienteId: 1, veiculoId: 10);
        ordem.Pecas.Add(new OrdemDeServicoPeca
        {
            OrdemDeServicoId = ordem.Id,
            PecaId = 1000,
            Quantidade = 1,
            Preco = 45m
        });

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _pecaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(1000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PecaMock.Criar(id: 1000, preco: 45m, quantidadeEstoque: 5));

        var acao = () => _service.RemoverPecaAsync(ordem.Id, 1000, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*remover pecas durante o diagnostico*");
    }

    [Fact]
    public async Task AdicionarServicoAsync_DeveLancarQuandoServicoJaExistirNaOs()
    {
        var ordem = OrdemDeServicoMock.Criar(status: StatusOrdemDeServico.EmDiagnostico, clienteId: 1, veiculoId: 10);
        var servico = ServicoMock.Criar(id: 1000, preco: 150m);
        ordem.Servicos.Add(new OrdemDeServicoServico
        {
            OrdemDeServicoId = ordem.Id,
            ServicoId = servico.Id,
            Preco = servico.Preco,
            TempoEstimado = servico.TempoEstimado
        });

        _ordemRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);
        _servicoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);
        var acao = () => _service.AdicionarServicoAsync(
            ordem.Id,
            new AdicionarServicoAOrdemDto { ServicoId = servico.Id },
            CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ja foi adicionado*");
    }
}
