using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class OrdemDeServicoHandlerDependencies
{
    public OrdemDeServicoHandlerDependencies(
        IOrdemDeServicoRepository ordemRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IPecaRepository pecaRepository,
        IPedidoCompraRepository pedidoCompraRepository,
        IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        INotificacaoClienteRepository notificacaoClienteRepository,
        ITransactionManager transactionManager,
        IClock clock,
        ILoggerFactory loggerFactory,
        OrdemDeServicoAcompanhamentoService acompanhamentoService,
        OrdemDeServicoHistoricoService historicoService,
        OrdemDeServicoNotificacaoService notificacaoService,
        OrdemDeServicoEstoqueService estoqueService)
    {
        OrdemRepository = ordemRepository;
        ClienteRepository = clienteRepository;
        VeiculoRepository = veiculoRepository;
        ServicoRepository = servicoRepository;
        PecaRepository = pecaRepository;
        PedidoCompraRepository = pedidoCompraRepository;
        MovimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
        HistoricoRepository = historicoRepository;
        NotificacaoClienteRepository = notificacaoClienteRepository;
        TransactionManager = transactionManager;
        Clock = clock;
        LoggerFactory = loggerFactory;
        AcompanhamentoService = acompanhamentoService;
        HistoricoService = historicoService;
        NotificacaoService = notificacaoService;
        EstoqueService = estoqueService;
    }

    public IOrdemDeServicoRepository OrdemRepository { get; }
    public IClienteRepository ClienteRepository { get; }
    public IVeiculoRepository VeiculoRepository { get; }
    public IServicoRepository ServicoRepository { get; }
    public IPecaRepository PecaRepository { get; }
    public IPedidoCompraRepository PedidoCompraRepository { get; }
    public IMovimentacaoEstoqueRepository MovimentacaoEstoqueRepository { get; }
    public IOrdemServicoHistoricoRepository HistoricoRepository { get; }
    public INotificacaoClienteRepository NotificacaoClienteRepository { get; }
    public ITransactionManager TransactionManager { get; }
    public IClock Clock { get; }
    public ILoggerFactory LoggerFactory { get; }
    public OrdemDeServicoAcompanhamentoService AcompanhamentoService { get; }
    public OrdemDeServicoHistoricoService HistoricoService { get; }
    public OrdemDeServicoNotificacaoService NotificacaoService { get; }
    public OrdemDeServicoEstoqueService EstoqueService { get; }
}

