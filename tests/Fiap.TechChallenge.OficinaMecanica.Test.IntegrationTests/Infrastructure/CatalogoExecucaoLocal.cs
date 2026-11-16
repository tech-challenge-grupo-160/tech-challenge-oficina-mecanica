using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

internal sealed class CatalogoExecucaoLocal : ICatalogoExecucao
{
    public Task<CatalogoExecucao> ObterItensAsync(
        IEnumerable<int> servicoIds,
        IEnumerable<int> pecaIds,
        CancellationToken cancellationToken)
    {
        var servicoIdsMaterializados = servicoIds.ToArray();
        var pecaIdsMaterializados = pecaIds.ToArray();
        var servicosEncontrados = servicoIdsMaterializados
            .Where(id => id == CustomWebApplicationFactory.ServicoExistenteId)
            .Select(id => new ServicoCatalogo(id, "Alinhamento", 150m, 30))
            .ToArray();
        var pecasEncontradas = pecaIdsMaterializados
            .Where(id => id == CustomWebApplicationFactory.PecaExistenteId)
            .Select(id => new PecaCatalogo(id, "Pastilha de Freio", 45m))
            .ToArray();

        return Task.FromResult(new CatalogoExecucao(servicosEncontrados, pecasEncontradas));
    }
}
