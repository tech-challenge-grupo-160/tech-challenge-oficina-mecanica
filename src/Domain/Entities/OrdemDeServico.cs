using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class OrdemDeServico
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public string DescricaoSolicitacao { get; set; } = null!;
    public string? ObservacoesRecepcao { get; set; }
    public string? MotivoCancelamento { get; set; }
    public DateTime? OrcamentoEnviadoEm { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusOrdemDeServico Status { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataConclusao { get; set; }
    public decimal ValorTotal { get; set; }

    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
    public ICollection<OrdemDeServicoServico> Servicos { get; set; } = new List<OrdemDeServicoServico>();
    public ICollection<OrdemDeServicoPeca> Pecas { get; set; } = new List<OrdemDeServicoPeca>();
    public ICollection<OrdemServicoHistorico> Historicos { get; set; } = new List<OrdemServicoHistorico>();

    public void AdicionarServico(Servico servico)
    {
        if (Status != StatusOrdemDeServico.EmDiagnostico)
        {
            throw new InvalidOperationException("So e possivel adicionar servicos durante o diagnostico. Nao e possivel neste status: " + Status);
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
        if (Status != StatusOrdemDeServico.EmDiagnostico &&
            Status != StatusOrdemDeServico.AguardandoAprovacao &&
            Status != StatusOrdemDeServico.AguardandoEstoque)
        {
            throw new InvalidOperationException($"Nao e possivel adicionar pecas neste status: {Status}");
        }

        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade da peca deve ser maior que zero.");
        }

        var itemExistente = Pecas.FirstOrDefault(item => item.PecaId == peca.Id);
        if (itemExistente == null)
        {
            Pecas.Add(new OrdemDeServicoPeca
            {
                OrdemDeServicoId = Id,
                PecaId = peca.Id,
                Quantidade = quantidade,
                Preco = peca.Preco
            });
        }
        else
        {
            itemExistente.Quantidade += quantidade;
            itemExistente.Preco = peca.Preco;
        }

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

    public void FinalizarDiagnostico()
    {
        if (Status != StatusOrdemDeServico.EmDiagnostico)
        {
            throw new InvalidOperationException("So e possivel finalizar o diagnostico quando a ordem estiver em diagnostico.");
        }

        if (!Servicos.Any())
        {
            throw new InvalidOperationException("A ordem de servico deve possuir ao menos um servico antes de aguardar aprovacao.");
        }

        if (ValorTotal <= 0)
        {
            throw new InvalidOperationException("O orcamento da ordem de servico nao pode ser zerado.");
        }

        AlterarStatus(StatusOrdemDeServico.AguardandoAprovacao);
        OrcamentoEnviadoEm = DateTimeHelper.UTCBrazilNow();
    }

    public void Cancelar(string motivoCancelamento)
    {
        if (Status != StatusOrdemDeServico.Recebida &&
            Status != StatusOrdemDeServico.EmDiagnostico &&
            Status != StatusOrdemDeServico.AguardandoAprovacao &&
            Status != StatusOrdemDeServico.AguardandoEstoque)
        {
            throw new InvalidOperationException("Nao e possivel cancelar a ordem de servico no status atual: " + Status);
        }

        if (string.IsNullOrWhiteSpace(motivoCancelamento))
        {
            throw new InvalidOperationException("Motivo do cancelamento e obrigatorio.");
        }

        Status = StatusOrdemDeServico.Cancelada;
        MotivoCancelamento = motivoCancelamento.Trim();
    }

    public void AprovarOrcamento()
    {
        if (Status != StatusOrdemDeServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException("So e possivel aprovar o orcamento quando a ordem estiver aguardando aprovacao.");
        }
    }

    public void BloquearPorFaltaEstoque()
    {
        if (Status != StatusOrdemDeServico.AguardandoAprovacao &&
            Status != StatusOrdemDeServico.AguardandoEstoque)
        {
            throw new InvalidOperationException("So e possivel aguardar estoque quando a ordem estiver aguardando aprovacao ou ja aguardando estoque.");
        }

        Status = StatusOrdemDeServico.AguardandoEstoque;
    }

    public void LiberarExecucaoAposValidacaoEstoque()
    {
        if (Status != StatusOrdemDeServico.AguardandoAprovacao &&
            Status != StatusOrdemDeServico.AguardandoEstoque)
        {
            throw new InvalidOperationException("So e possivel iniciar execucao apos aprovacao e validacao do estoque.");
        }

        AlterarStatus(StatusOrdemDeServico.EmExecucao);
    }

    public void FinalizarServico()
    {
        if (Status != StatusOrdemDeServico.EmExecucao)
        {
            throw new InvalidOperationException("So e possivel finalizar o servico quando a ordem estiver em execucao.");
        }

        DataFinalizacao = DateTimeHelper.UTCBrazilNow();
        AlterarStatus(StatusOrdemDeServico.Finalizada);
    }

    public void RegistrarPagamento()
    {
        if (Status != StatusOrdemDeServico.Finalizada)
        {
            throw new InvalidOperationException("So e possivel registrar pagamento quando a ordem estiver finalizada.");
        }

        if (!DataFinalizacao.HasValue)
        {
            throw new InvalidOperationException("Nao e possivel registrar pagamento antes da finalizacao do servico.");
        }

        DataPagamento = DateTimeHelper.UTCBrazilNow();
    }

    public void Entregar()
    {
        if (Status != StatusOrdemDeServico.Finalizada)
        {
            throw new InvalidOperationException("So e possivel entregar quando a ordem estiver finalizada.");
        }

        if (!DataPagamento.HasValue)
        {
            throw new InvalidOperationException("So e possivel entregar apos o pagamento ser registrado.");
        }

        AlterarStatus(StatusOrdemDeServico.Entregue);
    }

    private bool ValidarTransicaoDeStatus(StatusOrdemDeServico statusAtual, StatusOrdemDeServico novoStatus)
    {
        return (statusAtual, novoStatus) switch
        {
            (StatusOrdemDeServico.Recebida, StatusOrdemDeServico.EmDiagnostico) => true,
            (StatusOrdemDeServico.EmDiagnostico, StatusOrdemDeServico.AguardandoAprovacao) => true,
            (StatusOrdemDeServico.AguardandoAprovacao, StatusOrdemDeServico.EmExecucao) => true,
            (StatusOrdemDeServico.AguardandoEstoque, StatusOrdemDeServico.EmExecucao) => true,
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
