using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Catalogo;

public sealed class CatalogoExecucaoHttpClient(HttpClient httpClient) : ICatalogoExecucao
{
    public async Task<CatalogoExecucao> ObterItensAsync(
        IEnumerable<int> servicoIds,
        IEnumerable<int> pecaIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new ConsultarCatalogoRequest(servicoIds.Distinct().ToArray(), pecaIds.Distinct().ToArray());
            using var response = await httpClient.PostAsJsonAsync(
                "api/v1/catalogo/itens/consultar",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceUnavailableException(
                    $"Catalogo da Execucao respondeu com status {(int)response.StatusCode}.");
            }

            var catalogo = await response.Content.ReadFromJsonAsync<ConsultarCatalogoResponse>(
                cancellationToken: cancellationToken);
            if (catalogo?.Servicos is null || catalogo.Pecas is null)
                throw new ServiceUnavailableException("Catalogo da Execucao retornou uma resposta vazia.");

            if (catalogo.Servicos.Any(item =>
                    item.Id <= 0 || string.IsNullOrWhiteSpace(item.Nome) ||
                    item.Preco < 0 || item.TempoEstimado <= 0) ||
                catalogo.Pecas.Any(item =>
                    item.Id <= 0 || string.IsNullOrWhiteSpace(item.Nome) || item.Preco < 0))
            {
                throw new ServiceUnavailableException("Catalogo da Execucao retornou dados invalidos.");
            }

            return new CatalogoExecucao(
                catalogo.Servicos
                    .Select(item => new ServicoCatalogo(item.Id, item.Nome, item.Preco, item.TempoEstimado))
                    .ToArray(),
                catalogo.Pecas
                    .Select(item => new PecaCatalogo(item.Id, item.Nome, item.Preco))
                    .ToArray());
        }
        catch (ServiceUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException or JsonException or
                TimeoutRejectedException or BrokenCircuitException)
        {
            throw new ServiceUnavailableException("Nao foi possivel consultar o catalogo da Execucao.");
        }
    }

    private sealed record ConsultarCatalogoRequest(
        IReadOnlyCollection<int> ServicoIds,
        IReadOnlyCollection<int> PecaIds);

    private sealed record ConsultarCatalogoResponse(
        IReadOnlyList<ServicoResponse>? Servicos,
        IReadOnlyList<PecaResponse>? Pecas,
        IReadOnlyList<int> ServicoIdsNaoEncontrados,
        IReadOnlyList<int> PecaIdsNaoEncontrados);

    private sealed record ServicoResponse(int Id, string Nome, decimal Preco, int TempoEstimado);
    private sealed record PecaResponse(int Id, string Nome, decimal Preco);
}
