namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface ICatalogoExecucao
{
    Task<CatalogoExecucao> ObterItensAsync(
        IEnumerable<int> servicoIds,
        IEnumerable<int> pecaIds,
        CancellationToken cancellationToken);
}

public sealed record CatalogoExecucao(
    IReadOnlyList<ServicoCatalogo> Servicos,
    IReadOnlyList<PecaCatalogo> Pecas);

public sealed record ServicoCatalogo(int Id, string Nome, decimal Preco, int TempoEstimado);

public sealed record PecaCatalogo(int Id, string Nome, decimal Preco);
