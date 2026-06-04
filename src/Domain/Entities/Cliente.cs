using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Cliente
{
    private Cliente()
    {
    }

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string CpfCnpj { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateTime DataCadastro { get; private set; }

    public ICollection<Veiculo> Veiculos { get; private set; } = new List<Veiculo>();
    public ICollection<OrdemDeServico> OrdensDeServico { get; private set; } = new List<OrdemDeServico>();

    public static Cliente Criar(string nome, Documento documento, Telefone telefone, string email, DateTime dataCadastro)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("E-mail e obrigatorio.");
        }

        return new Cliente
        {
            Nome = nome.Trim(),
            CpfCnpj = documento.Valor,
            Telefone = telefone.Valor,
            Email = email.Trim(),
            DataCadastro = dataCadastro
        };
    }

    public void AtualizarContato(string nome, Telefone telefone, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("E-mail e obrigatorio.");
        }

        Nome = nome.Trim();
        Telefone = telefone.Valor;
        Email = email.Trim();
    }
}
