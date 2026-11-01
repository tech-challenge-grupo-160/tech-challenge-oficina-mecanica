using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

internal sealed class CatalogoExecucaoLocal(
    IServicoRepository servicos,
    IPecaRepository pecas) : ICatalogoExecucao
{
    public async Task<CatalogoExecucao> ObterItensAsync(
        IEnumerable<int> servicoIds,
        IEnumerable<int> pecaIds,
        CancellationToken cancellationToken)
    {
        var servicoIdsMaterializados = servicoIds.ToArray();
        var pecaIdsMaterializados = pecaIds.ToArray();
        var servicosEncontrados = servicoIdsMaterializados.Length == 0
            ? []
            : await servicos.ObterPorIdsAsync(servicoIdsMaterializados, cancellationToken);
        var pecasEncontradas = pecaIdsMaterializados.Length == 0
            ? []
            : await pecas.ObterPorIdsAsync(pecaIdsMaterializados, cancellationToken);

        return new CatalogoExecucao(
            servicosEncontrados
                .Select(item => new ServicoCatalogo(item.Id, item.Nome, item.Preco, item.TempoEstimado))
                .ToArray(),
            pecasEncontradas
                .Select(item => new PecaCatalogo(item.Id, item.Nome, item.Preco))
                .ToArray());
    }
}
