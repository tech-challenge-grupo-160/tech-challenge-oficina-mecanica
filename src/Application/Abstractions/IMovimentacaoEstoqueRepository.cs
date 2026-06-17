using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.abstractions;

public interface IMovimentacaoEstoqueRepository
{
    Task<MovimentacaoEstoque> CriarAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken);
    Task<IEnumerable<MovimentacaoEstoque>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken);
}
