using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string CpfCnpj { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DataCadastro { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<OrdemDeServico> OrdensDeServico { get; set; } = new List<OrdemDeServico>();

    public static Cliente Criar(string nome, Documento documento, Telefone telefone, string email)
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
            DataCadastro = DateTimeHelper.UTCBrazilNow()
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
