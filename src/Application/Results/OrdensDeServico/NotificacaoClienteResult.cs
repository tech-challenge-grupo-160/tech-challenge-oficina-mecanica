namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

public sealed class NotificacaoClienteResult
{
    public int Id { get; init; }
    public int OrdemDeServicoId { get; init; }
    public string Canal { get; init; } = null!;
    public string TipoNotificacao { get; init; } = null!;
    public string Mensagem { get; init; } = null!;
    public bool Recebida { get; init; }
    public DateTime DataNotificacao { get; init; }
}
