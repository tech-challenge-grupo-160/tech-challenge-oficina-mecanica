using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class NotificacaoCliente
{
    public int Id { get; set; }
    public int OrdemDeServicoId { get; set; }
    public DateTime DataNotificacao { get; set; }
    public CanalNotificacaoCliente Canal { get; set; }
    public TipoNotificacaoCliente TipoNotificacao { get; set; }
    public string Mensagem { get; set; } = null!;
    public bool Recebida { get; set; }

    public OrdemDeServico? OrdemDeServico { get; set; }
}
