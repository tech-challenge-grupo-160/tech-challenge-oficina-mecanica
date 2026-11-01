namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface IDiagnosticoExecucao
{
    Task<DiagnosticoExecucao> IniciarAsync(
        int ordemServicoId,
        int veiculoId,
        string descricaoSolicitacao,
        IEnumerable<int> servicoIdsSolicitados,
        IEnumerable<int> pecaIdsSolicitadas,
        CancellationToken cancellationToken);
}

public sealed record DiagnosticoExecucao(
    int Id,
    int OrdemServicoId,
    string Status,
    DateTimeOffset IniciadoEm);
