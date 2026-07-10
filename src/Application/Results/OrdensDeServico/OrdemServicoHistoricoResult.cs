namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

public sealed class OrdemServicoHistoricoResult
{
    public int Id { get; init; }
    public int OrdemDeServicoId { get; init; }
    public string? UsuarioId { get; init; }
    public string? UsuarioNome { get; init; }
    public string? StatusAnterior { get; init; }
    public string? StatusNovo { get; init; }
    public string TipoEvento { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public DateTime DataEvento { get; init; }
}
