namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class MonitoramentoOrdemDeServicoDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public bool EstaFinalizada { get; set; }
    public int TempoDecorridoMinutos { get; set; }
    public double TempoDecorridoHoras { get; set; }
    public int? TempoFinalizacaoMinutos { get; set; }
    public double? TempoFinalizacaoHoras { get; set; }
}
