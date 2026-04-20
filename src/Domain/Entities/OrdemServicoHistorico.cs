using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class OrdemServicoHistorico
{
    public int Id { get; set; }
    public int OrdemDeServicoId { get; set; }
    public string? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public StatusOrdemDeServico? StatusAnterior { get; set; }
    public StatusOrdemDeServico? StatusNovo { get; set; }
    public TipoEventoOrdemServico TipoEvento { get; set; }
    public string Descricao { get; set; } = null!;
    public DateTime DataEvento { get; set; }

    public OrdemDeServico? OrdemDeServico { get; set; }
}
