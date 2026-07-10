using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Cliente
{
    private Cliente()
    {
    }

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public Documento CpfCnpj { get; private set; }
    public Telefone Telefone { get; private set; }
    public Email Email { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public ICollection<Veiculo> Veiculos { get; private set; } = new List<Veiculo>();
    public ICollection<OrdemDeServico> OrdensDeServico { get; private set; } = new List<OrdemDeServico>();

    public static Cliente Criar(string nome, Documento documento, Telefone telefone, Email email, DateTime dataCadastro)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome e obrigatorio.");
        }

        return new Cliente
        {
            Nome = nome.Trim(),
            CpfCnpj = documento,
            Telefone = telefone,
            Email = email,
            DataCadastro = dataCadastro
        };
    }

    public void AtualizarContato(string nome, Telefone telefone, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome e obrigatorio.");
        }

        Nome = nome.Trim();
        Telefone = telefone;
        Email = email;
    }
}
