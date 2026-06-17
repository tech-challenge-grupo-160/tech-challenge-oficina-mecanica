using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Veiculo
{
    private Veiculo()
    {
    }

    public int Id { get; private set; }
    public string Placa { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public int Ano { get; private set; }
    public int ClienteId { get; private set; }

    public Cliente? Cliente { get; private set; }
    public ICollection<OrdemDeServico> OrdensDeServico { get; private set; } = new List<OrdemDeServico>();

    public static Veiculo Criar(PlacaVeiculo placa, string marca, string modelo, int ano, int clienteId)
    {
        ValidarDados(marca, modelo, ano, clienteId);

        return new Veiculo
        {
            Placa = placa.Valor,
            Marca = marca.Trim(),
            Modelo = modelo.Trim(),
            Ano = ano,
            ClienteId = clienteId
        };
    }

    public void AtualizarDados(string marca, string modelo, int ano)
    {
        ValidarDados(marca, modelo, ano, ClienteId);

        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Ano = ano;
    }

    private static void ValidarDados(string marca, string modelo, int ano, int clienteId)
    {
        if (string.IsNullOrWhiteSpace(marca))
        {
            throw new ArgumentException("Marca e obrigatoria.");
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ArgumentException("Modelo e obrigatorio.");
        }

        if (ano < 1900 || ano > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Ano do veiculo invalido.");
        }

        if (clienteId <= 0)
        {
            throw new ArgumentException("Cliente do veiculo invalido.");
        }
    }
}
