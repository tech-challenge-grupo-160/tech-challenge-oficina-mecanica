namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class NotificacaoClienteDto
{
    public int Id { get; set; }
    public int OrdemDeServicoId { get; set; }
    public string Canal { get; set; } = null!;
    public string TipoNotificacao { get; set; } = null!;
    public string Mensagem { get; set; } = null!;
    public bool Recebida { get; set; }
    public DateTime DataNotificacao { get; set; }
}
