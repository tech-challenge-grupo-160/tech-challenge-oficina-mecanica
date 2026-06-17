using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.abstractions;

public interface IPedidoCompraRepository
{
    Task<PedidoCompra?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<PedidoCompra?> ObterPendentePorOrdemEPecaAsync(int ordemDeServicoId, int pecaId, CancellationToken cancellationToken);
    Task<int> ContarAsync(CancellationToken cancellationToken);
    Task<IEnumerable<PedidoCompra>> ObterPaginadoAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<PedidoCompra>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken);
    Task<PedidoCompra> CriarAsync(PedidoCompra pedidoCompra, CancellationToken cancellationToken);
    Task<PedidoCompra> AtualizarAsync(PedidoCompra pedidoCompra, CancellationToken cancellationToken);
}
