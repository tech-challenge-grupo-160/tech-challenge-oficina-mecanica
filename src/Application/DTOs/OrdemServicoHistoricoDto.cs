namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class OrdemServicoHistoricoDto
{
    public int Id { get; set; }
    public int OrdemDeServicoId { get; set; }
    public string? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public string? StatusAnterior { get; set; }
    public string? StatusNovo { get; set; }
    public string TipoEvento { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public DateTime DataEvento { get; set; }
}
