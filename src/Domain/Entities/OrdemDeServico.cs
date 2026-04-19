using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public enum StatusOrdemDeServico
{
    Recebida = 0,
    EmDiagnostico = 1,
    AguardandoAprovacao = 2,
    EmExecucao = 3,
    Finalizada = 4,
    Entregue = 5,
    Cancelada = 6
}

public class OrdemDeServico
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public string DescricaoSolicitacao { get; set; } = null!;
    public string? ObservacoesRecepcao { get; set; }
    public string? MotivoCancelamento { get; set; }
    public StatusOrdemDeServico Status { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataConclusao { get; set; }
    public decimal ValorTotal { get; set; }

    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
    public ICollection<OrdemDeServicoServico> Servicos { get; set; } = new List<OrdemDeServicoServico>();
    public ICollection<OrdemDeServicoPeca> Pecas { get; set; } = new List<OrdemDeServicoPeca>();

    public void AdicionarServico(Servico servico)
    {
        if (Status != StatusOrdemDeServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException("So e possivel adicionar servicos em ordens aguardando aprovacao.");
        }

        Servicos.Add(new OrdemDeServicoServico
        {
            OrdemDeServicoId = Id,
            ServicoId = servico.Id,
            Preco = servico.Preco,
            TempoEstimado = servico.TempoEstimado
        });

        RecalcularTotal();
    }

    public void AdicionarPeca(Peca peca, int quantidade)
    {
        if (Status != StatusOrdemDeServico.AguardandoAprovacao && Status != StatusOrdemDeServico.EmExecucao)
        {
            throw new InvalidOperationException("Nao e possivel adicionar pecas neste estado.");
        }

        if (peca.QuantidadeEstoque < quantidade)
        {
            throw new InvalidOperationException("Quantidade insuficiente em estoque.");
        }

        Pecas.Add(new OrdemDeServicoPeca
        {
            OrdemDeServicoId = Id,
            PecaId = peca.Id,
            Quantidade = quantidade,
            Preco = peca.Preco
        });

        peca.QuantidadeEstoque -= quantidade;
        RecalcularTotal();
    }

    public void AlterarStatus(StatusOrdemDeServico novoStatus)
    {
        if (!ValidarTransicaoDeStatus(Status, novoStatus))
        {
            throw new InvalidOperationException($"Nao e possivel transicionar de {Status} para {novoStatus}");
        }

        Status = novoStatus;

        if (novoStatus == StatusOrdemDeServico.Entregue)
        {
            DataConclusao = DateTimeHelper.UTCBrazilNow();
        }
    }

    public void Cancelar(string motivoCancelamento)
    {
        if (Status != StatusOrdemDeServico.Recebida &&
            Status != StatusOrdemDeServico.EmDiagnostico &&
            Status != StatusOrdemDeServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException("Nao e possivel cancelar a ordem de servico no status atual.");
        }

        if (string.IsNullOrWhiteSpace(motivoCancelamento))
        {
            throw new InvalidOperationException("Motivo do cancelamento e obrigatorio.");
        }

        Status = StatusOrdemDeServico.Cancelada;
        MotivoCancelamento = motivoCancelamento.Trim();
    }

    private bool ValidarTransicaoDeStatus(StatusOrdemDeServico statusAtual, StatusOrdemDeServico novoStatus)
    {
        return (statusAtual, novoStatus) switch
        {
            (StatusOrdemDeServico.Recebida, StatusOrdemDeServico.EmDiagnostico) => true,
            (StatusOrdemDeServico.EmDiagnostico, StatusOrdemDeServico.AguardandoAprovacao) => true,
            (StatusOrdemDeServico.AguardandoAprovacao, StatusOrdemDeServico.EmExecucao) => true,
            (StatusOrdemDeServico.EmExecucao, StatusOrdemDeServico.Finalizada) => true,
            (StatusOrdemDeServico.Finalizada, StatusOrdemDeServico.Entregue) => true,
            _ => false
        };
    }

    private void RecalcularTotal()
    {
        decimal totalServicos = Servicos.Sum(s => s.Preco);
        decimal totalPecas = Pecas.Sum(p => p.Quantidade * p.Preco);
        ValorTotal = totalServicos + totalPecas;
    }
}

public class OrdemDeServicoServico
{
    public int OrdemDeServicoId { get; set; }
    public int ServicoId { get; set; }
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }

    public OrdemDeServico? OrdemDeServico { get; set; }
    public Servico? Servico { get; set; }
}

public class OrdemDeServicoPeca
{
    public int OrdemDeServicoId { get; set; }
    public int PecaId { get; set; }
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }

    public OrdemDeServico? OrdemDeServico { get; set; }
    public Peca? Peca { get; set; }
}
