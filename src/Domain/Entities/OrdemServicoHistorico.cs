using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class OrdemServicoHistorico
{
    private OrdemServicoHistorico()
    {
    }

    public int Id { get; private set; }
    public int OrdemDeServicoId { get; private set; }
    public string? UsuarioId { get; private set; }
    public string? UsuarioNome { get; private set; }
    public StatusOrdemDeServico? StatusAnterior { get; private set; }
    public StatusOrdemDeServico? StatusNovo { get; private set; }
    public TipoEventoOrdemServico TipoEvento { get; private set; }
    public string Descricao { get; private set; } = null!;
    public DateTime DataEvento { get; private set; }

    public OrdemDeServico? OrdemDeServico { get; private set; }

    public static OrdemServicoHistorico Registrar(
        int ordemDeServicoId,
        string? usuarioId,
        string? usuarioNome,
        StatusOrdemDeServico? statusAnterior,
        StatusOrdemDeServico? statusNovo,
        TipoEventoOrdemServico tipoEvento,
        string descricao,
        DateTime dataEvento)
    {
        if (ordemDeServicoId <= 0)
        {
            throw new ArgumentException("Ordem de servico do historico e obrigatoria.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("Descricao do historico e obrigatoria.");
        }

        return new OrdemServicoHistorico
        {
            OrdemDeServicoId = ordemDeServicoId,
            UsuarioId = usuarioId,
            UsuarioNome = usuarioNome,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            TipoEvento = tipoEvento,
            Descricao = descricao.Trim(),
            DataEvento = dataEvento
        };
    }
}
