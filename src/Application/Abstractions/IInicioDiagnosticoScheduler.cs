namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface IInicioDiagnosticoScheduler
{
    Task<Guid> AgendarAsync(
        int ordemServicoId,
        int veiculoId,
        string descricaoSolicitacao,
        IReadOnlyCollection<int> servicoIdsSolicitados,
        IReadOnlyCollection<int> pecaIdsSolicitadas,
        CancellationToken cancellationToken);
}
