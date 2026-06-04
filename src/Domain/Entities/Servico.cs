namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Servico
{
    private Servico()
    {
    }

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public int TempoEstimado { get; private set; }

    public static Servico Criar(string nome, string descricao, decimal preco, int tempoEstimado)
    {
        ValidarDados(nome, descricao, preco, tempoEstimado);

        return new Servico
        {
            Nome = nome.Trim(),
            Descricao = descricao.Trim(),
            Preco = preco,
            TempoEstimado = tempoEstimado
        };
    }

    public void AtualizarDados(string nome, string descricao, decimal preco, int tempoEstimado)
    {
        ValidarDados(nome, descricao, preco, tempoEstimado);

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Preco = preco;
        TempoEstimado = tempoEstimado;
    }

    // Navigation
    public ICollection<OrdemDeServicoServico> OrdensDeServico { get; private set; } = new List<OrdemDeServicoServico>();

    private static void ValidarDados(string nome, string descricao, decimal preco, int tempoEstimado)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do servico e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("Descricao do servico e obrigatoria.");
        }

        if (preco < 0)
        {
            throw new ArgumentException("Preco do servico nao pode ser negativo.");
        }

        if (tempoEstimado <= 0)
        {
            throw new ArgumentException("Tempo estimado do servico deve ser maior que zero.");
        }
    }
}
