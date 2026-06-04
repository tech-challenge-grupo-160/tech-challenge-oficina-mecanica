using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class NotificacaoCliente
{
    private NotificacaoCliente()
    {
    }

    public int Id { get; private set; }
    public int OrdemDeServicoId { get; private set; }
    public DateTime DataNotificacao { get; private set; }
    public CanalNotificacaoCliente Canal { get; private set; }
    public TipoNotificacaoCliente TipoNotificacao { get; private set; }
    public string Mensagem { get; private set; } = null!;
    public bool Recebida { get; private set; }

    public OrdemDeServico? OrdemDeServico { get; private set; }

    public static NotificacaoCliente Criar(
        int ordemDeServicoId,
        DateTime dataNotificacao,
        CanalNotificacaoCliente canal,
        TipoNotificacaoCliente tipoNotificacao,
        string mensagem,
        bool recebida)
    {
        if (ordemDeServicoId <= 0)
        {
            throw new ArgumentException("Ordem de servico da notificacao e obrigatoria.");
        }

        if (string.IsNullOrWhiteSpace(mensagem))
        {
            throw new ArgumentException("Mensagem da notificacao e obrigatoria.");
        }

        return new NotificacaoCliente
        {
            OrdemDeServicoId = ordemDeServicoId,
            DataNotificacao = dataNotificacao,
            Canal = canal,
            TipoNotificacao = tipoNotificacao,
            Mensagem = mensagem.Trim(),
            Recebida = recebida
        };
    }
}
