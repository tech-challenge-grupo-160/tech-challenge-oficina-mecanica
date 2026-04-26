using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class OrdemDeServico
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
    public string CodigoAcompanhamento { get; set; } = null!;
    public string TokenAcompanhamentoHash { get; set; } = null!;
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
    public ICollection<NotificacaoCliente> NotificacoesCliente { get; set; } = new List<NotificacaoCliente>();

    public OrdemDeServicoEventoDominio CriarEventoOrdemCriada()
    {
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.OrdemCriada,
            null,
            Status,
            "Ordem de servico criada.");
    }

    public OrdemDeServicoEventoDominio IniciarDiagnostico()
    {
        var statusAnterior = Status;
        AlterarStatus(StatusOrdemDeServico.EmDiagnostico);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.DiagnosticoIniciado,
            statusAnterior,
            Status,
            "Diagnostico iniciado.");
    }

    public void AdicionarServico(Servico servico)
    {
        if (Status != StatusOrdemDeServico.EmDiagnostico)
        {
            throw new InvalidOperationException("So e possivel adicionar servicos durante o diagnostico. Nao e possivel neste status: " + Status);
        }

        if (Servicos.Any(x => x.ServicoId == servico.Id))
        {
            throw new InvalidOperationException("O servico informado ja foi adicionado a ordem de servico.");
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

    public OrdemDeServicoEventoDominio AdicionarServicoComEvento(Servico servico)
    {
        AdicionarServico(servico);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.ServicoAdicionado,
            Status,
            Status,
            $"Servico adicionado ao orcamento: {servico.Nome}.");
    }

    public void RemoverServico(int servicoId)
    {
        if (Status != StatusOrdemDeServico.EmDiagnostico)
        {
            throw new InvalidOperationException("So e possivel remover servicos durante o diagnostico. Nao e possivel neste status: " + Status);
        }

        var item = Servicos.FirstOrDefault(x => x.ServicoId == servicoId);
        if (item == null)
        {
            throw new KeyNotFoundException($"Servico com ID {servicoId} nao encontrado na ordem de servico.");
        }

        Servicos.Remove(item);
        RecalcularTotal();
    }

    public OrdemDeServicoEventoDominio RemoverServicoComEvento(int servicoId, string nomeServico)
    {
        RemoverServico(servicoId);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.ServicoAdicionado,
            Status,
            Status,
            $"Servico removido do orcamento: {nomeServico}.");
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

    public OrdemDeServicoEventoDominio AdicionarPecaComEvento(Peca peca, int quantidade)
    {
        AdicionarPeca(peca, quantidade);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.PecaAdicionada,
            Status,
            Status,
            $"Peca adicionada ao orcamento: {peca.Nome}. Quantidade: {quantidade}.");
    }

    public void RemoverPeca(int pecaId)
    {
        if (Status != StatusOrdemDeServico.EmDiagnostico)
        {
            throw new InvalidOperationException("So e possivel remover pecas durante o diagnostico. Nao e possivel neste status: " + Status);
        }

        var item = Pecas.FirstOrDefault(x => x.PecaId == pecaId);
        if (item == null)
        {
            throw new KeyNotFoundException($"Peca com ID {pecaId} nao encontrada na ordem de servico.");
        }

        Pecas.Remove(item);
        RecalcularTotal();
    }

    public OrdemDeServicoEventoDominio RemoverPecaComEvento(int pecaId, string nomePeca)
    {
        RemoverPeca(pecaId);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.PecaAdicionada,
            Status,
            Status,
            $"Peca removida do orcamento: {nomePeca}.");
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

    public OrdemDeServicoEventoDominio FinalizarDiagnosticoComEvento()
    {
        var statusAnterior = Status;
        FinalizarDiagnostico();
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.DiagnosticoFinalizado,
            statusAnterior,
            Status,
            "Diagnostico finalizado e orcamento enviado para aprovacao.");
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

    public OrdemDeServicoEventoDominio CancelarComEvento(string motivoCancelamento)
    {
        var statusAnterior = Status;
        Cancelar(motivoCancelamento);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.OrdemCancelada,
            statusAnterior,
            Status,
            $"Ordem cancelada. Motivo: {MotivoCancelamento}");
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

    public OrdemDeServicoEventoDominio BloquearPorFaltaEstoqueComEvento(string descricaoFaltas)
    {
        var statusAnterior = Status;
        BloquearPorFaltaEstoque();
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.BloqueioPorFaltaEstoque,
            statusAnterior,
            Status,
            $"Execucao bloqueada por falta de estoque: {descricaoFaltas}");
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

    public OrdemDeServicoEventoDominio LiberarExecucaoComEvento()
    {
        var statusAnterior = Status;
        LiberarExecucaoAposValidacaoEstoque();
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.OrcamentoAprovado,
            statusAnterior,
            Status,
            "Orcamento aprovado pelo cliente e estoque validado com sucesso.");
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

    public OrdemDeServicoEventoDominio FinalizarServicoComEvento()
    {
        var statusAnterior = Status;
        FinalizarServico();
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.ServicoFinalizado,
            statusAnterior,
            Status,
            "Servico finalizado.");
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

    public OrdemDeServicoEventoDominio RegistrarPagamentoComEvento()
    {
        RegistrarPagamento();
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.PagamentoRegistrado,
            Status,
            Status,
            "Pagamento registrado para a ordem de servico.");
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

    public OrdemDeServicoEventoDominio EntregarComEvento()
    {
        var statusAnterior = Status;
        Entregar();
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.VeiculoEntregue,
            statusAnterior,
            Status,
            "Veiculo entregue ao cliente.");
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

public sealed record OrdemDeServicoEventoDominio(
    TipoEventoOrdemServico TipoEvento,
    StatusOrdemDeServico? StatusAnterior,
    StatusOrdemDeServico? StatusNovo,
    string Descricao);

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
