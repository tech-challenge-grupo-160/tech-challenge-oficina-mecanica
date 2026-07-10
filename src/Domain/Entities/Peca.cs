namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Peca
{
    private Peca()
    {
    }

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public int QuantidadeEstoque { get; private set; }

    public static Peca Criar(string nome, string marca, string modelo, decimal preco, int quantidadeEstoque)
    {
        ValidarDados(nome, marca, modelo, preco, quantidadeEstoque);

        return new Peca
        {
            Nome = nome.Trim(),
            Marca = marca.Trim(),
            Modelo = modelo.Trim(),
            Preco = preco,
            QuantidadeEstoque = quantidadeEstoque
        };
    }

    public void AtualizarDados(string nome, string marca, string modelo, decimal preco, int quantidadeEstoque)
    {
        ValidarDados(nome, marca, modelo, preco, quantidadeEstoque);

        Nome = nome.Trim();
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Preco = preco;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public void BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade para baixa deve ser maior que zero.");
        }

        if (QuantidadeEstoque < quantidade)
        {
            throw new InvalidOperationException("Quantidade insuficiente em estoque.");
        }

        QuantidadeEstoque -= quantidade;
    }

    public void ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade para reposicao deve ser maior que zero.");
        }

        QuantidadeEstoque += quantidade;
    }

    public ICollection<OrdemDeServicoPeca> OrdensDeServico { get; private set; } = new List<OrdemDeServicoPeca>();
    public ICollection<PedidoCompra> PedidosCompra { get; private set; } = new List<PedidoCompra>();
    public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; private set; } = new List<MovimentacaoEstoque>();

    private static void ValidarDados(string nome, string marca, string modelo, decimal preco, int quantidadeEstoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome da peca e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(marca))
        {
            throw new ArgumentException("Marca da peca e obrigatoria.");
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ArgumentException("Modelo da peca e obrigatorio.");
        }

        if (preco < 0)
        {
            throw new ArgumentException("Preco da peca nao pode ser negativo.");
        }

        if (quantidadeEstoque < 0)
        {
            throw new ArgumentException("Quantidade em estoque nao pode ser negativa.");
        }
    }
}
