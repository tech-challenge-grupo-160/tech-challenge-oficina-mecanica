using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

public interface IPedidoCompraApplicationService
{
    Task<PedidoCompraDto> CriarAsync(CriarPedidoCompraDto dto, CancellationToken cancellationToken);
    Task<PagedResultDto<PedidoCompraDto>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<PedidoCompraDto>> ListarPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken);
    Task<PedidoCompraDto> RegistrarRecebimentoAsync(int pedidoCompraId, ReceberPedidoCompraDto dto, CancellationToken cancellationToken);
}
