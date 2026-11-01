using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Execucao;

public sealed class DiagnosticoExecucaoHttpClient(HttpClient httpClient) : IDiagnosticoExecucao
{
    public async Task<DiagnosticoExecucao> IniciarAsync(
        int ordemServicoId,
        int veiculoId,
        string descricaoSolicitacao,
        IEnumerable<int> servicoIdsSolicitados,
        IEnumerable<int> pecaIdsSolicitadas,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = new IniciarDiagnosticoRequest(
                ordemServicoId,
                veiculoId,
                descricaoSolicitacao,
                servicoIdsSolicitados.Distinct().Order().ToArray(),
                pecaIdsSolicitadas.Distinct().Order().ToArray());
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/diagnosticos")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("Idempotency-Key", $"iniciar-diagnostico:{ordemServicoId}");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceUnavailableException(
                    $"Execucao respondeu com status {(int)response.StatusCode} ao iniciar o diagnostico.");
            }

            var diagnostico = await response.Content.ReadFromJsonAsync<DiagnosticoResponse>(
                cancellationToken: cancellationToken);
            if (diagnostico is null || diagnostico.Id <= 0 ||
                diagnostico.OrdemServicoId != ordemServicoId ||
                !string.Equals(diagnostico.Status, "EmDiagnostico", StringComparison.Ordinal))
            {
                throw new ServiceUnavailableException(
                    "Execucao retornou dados invalidos ao iniciar o diagnostico.");
            }

            return new DiagnosticoExecucao(
                diagnostico.Id,
                diagnostico.OrdemServicoId,
                diagnostico.Status,
                diagnostico.IniciadoEm);
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
            throw new ServiceUnavailableException(
                "Nao foi possivel iniciar o diagnostico na Execucao.");
        }
    }

    private sealed record IniciarDiagnosticoRequest(
        int OrdemServicoId,
        int VeiculoId,
        string DescricaoSolicitacao,
        IReadOnlyCollection<int> ServicoIdsSolicitados,
        IReadOnlyCollection<int> PecaIdsSolicitadas);

    private sealed record DiagnosticoResponse(
        int Id,
        int OrdemServicoId,
        int VeiculoId,
        string DescricaoSolicitacao,
        IReadOnlyCollection<int> ServicoIdsSolicitados,
        IReadOnlyCollection<int> PecaIdsSolicitadas,
        string Status,
        DateTimeOffset IniciadoEm);
}
