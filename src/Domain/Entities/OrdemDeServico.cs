using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class OrdemDeServico
{
    private OrdemDeServico()
    {
    }

    public int Id { get; private set; }
    public string Numero { get; private set; } = null!;
    public string CodigoAcompanhamento { get; private set; } = null!;
    public string TokenAcompanhamentoHash { get; private set; } = null!;
    public int ClienteId { get; private set; }
    public int VeiculoId { get; private set; }
    public string DescricaoSolicitacao { get; private set; } = null!;
    public string? ObservacoesRecepcao { get; private set; }
    public string? MotivoCancelamento { get; private set; }
    public DateTime? OrcamentoEnviadoEm { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public DateTime? DataPagamento { get; private set; }
    public StatusOrdemDeServico Status { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public decimal ValorTotal { get; private set; }

    public Cliente? Cliente { get; private set; }
    public Veiculo? Veiculo { get; private set; }
    public ICollection<OrdemDeServicoServico> Servicos { get; private set; } = new List<OrdemDeServicoServico>();
    public ICollection<OrdemDeServicoPeca> Pecas { get; private set; } = new List<OrdemDeServicoPeca>();
    public ICollection<OrdemServicoHistorico> Historicos { get; private set; } = new List<OrdemServicoHistorico>();
    public ICollection<NotificacaoCliente> NotificacoesCliente { get; private set; } = new List<NotificacaoCliente>();

    public static OrdemDeServico Criar(
        string numero,
        string codigoAcompanhamento,
        string tokenAcompanhamentoHash,
        int clienteId,
        int veiculoId,
        string descricaoSolicitacao,
        string? observacoesRecepcao,
        DateTime dataAbertura)
    {
        ValidarDadosBase(numero, codigoAcompanhamento, tokenAcompanhamentoHash, clienteId, veiculoId, descricaoSolicitacao);

        return new OrdemDeServico
        {
            Numero = numero.Trim(),
            CodigoAcompanhamento = codigoAcompanhamento.Trim(),
            TokenAcompanhamentoHash = tokenAcompanhamentoHash.Trim(),
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            DescricaoSolicitacao = descricaoSolicitacao.Trim(),
            ObservacoesRecepcao = string.IsNullOrWhiteSpace(observacoesRecepcao) ? null : observacoesRecepcao.Trim(),
            Status = StatusOrdemDeServico.Recebida,
            DataAbertura = dataAbertura,
            ValorTotal = 0
        };
    }

    public static OrdemDeServico Restaurar(
        string numero,
        string codigoAcompanhamento,
        string tokenAcompanhamentoHash,
        int clienteId,
        int veiculoId,
        string descricaoSolicitacao,
        string? observacoesRecepcao,
        string? motivoCancelamento,
        StatusOrdemDeServico status,
        DateTime dataAbertura,
        DateTime? orcamentoEnviadoEm,
        DateTime? dataFinalizacao,
        DateTime? dataPagamento,
        DateTime? dataConclusao,
        decimal valorTotal)
    {
        ValidarDadosBase(numero, codigoAcompanhamento, tokenAcompanhamentoHash, clienteId, veiculoId, descricaoSolicitacao);

        if (valorTotal < 0)
        {
            throw new ArgumentException("Valor total da ordem de servico nao pode ser negativo.");
        }

        return new OrdemDeServico
        {
            Numero = numero.Trim(),
            CodigoAcompanhamento = codigoAcompanhamento.Trim(),
            TokenAcompanhamentoHash = tokenAcompanhamentoHash.Trim(),
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            DescricaoSolicitacao = descricaoSolicitacao.Trim(),
            ObservacoesRecepcao = string.IsNullOrWhiteSpace(observacoesRecepcao) ? null : observacoesRecepcao.Trim(),
            MotivoCancelamento = string.IsNullOrWhiteSpace(motivoCancelamento) ? null : motivoCancelamento.Trim(),
            Status = status,
            DataAbertura = dataAbertura,
            OrcamentoEnviadoEm = orcamentoEnviadoEm,
            DataFinalizacao = dataFinalizacao,
            DataPagamento = dataPagamento,
            DataConclusao = dataConclusao,
            ValorTotal = valorTotal
        };
    }

    public void DefinirNumero(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            throw new ArgumentException("Numero da ordem de servico e obrigatorio.");
        }

        Numero = numero.Trim();
    }

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

        Servicos.Add(OrdemDeServicoServico.Criar(Id, servico.Id, servico.Preco, servico.TempoEstimado));

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

    public OrdemDeServicoEventoDominio AdicionarServicoNaAberturaComEvento(Servico servico)
    {
        if (Status != StatusOrdemDeServico.Recebida)
        {
            throw new InvalidOperationException("So e possivel adicionar servicos na abertura quando a ordem estiver recebida.");
        }

        if (Servicos.Any(x => x.ServicoId == servico.Id))
        {
            throw new InvalidOperationException("O servico informado ja foi adicionado a ordem de servico.");
        }

        Servicos.Add(OrdemDeServicoServico.Criar(Id, servico.Id, servico.Preco, servico.TempoEstimado));
        RecalcularTotal();

        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.ServicoAdicionado,
            Status,
            Status,
            $"Servico adicionado na abertura: {servico.Nome}.");
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
            TipoEventoOrdemServico.ServicoRemovido,
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
            Pecas.Add(OrdemDeServicoPeca.Criar(Id, peca.Id, quantidade, peca.Preco));
        }
        else
        {
            itemExistente.SomarQuantidade(quantidade, peca.Preco);
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

    public OrdemDeServicoEventoDominio AdicionarPecaNaAberturaComEvento(Peca peca, int quantidade)
    {
        if (Status != StatusOrdemDeServico.Recebida)
        {
            throw new InvalidOperationException("So e possivel adicionar pecas na abertura quando a ordem estiver recebida.");
        }

        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade da peca deve ser maior que zero.");
        }

        var itemExistente = Pecas.FirstOrDefault(item => item.PecaId == peca.Id);
        if (itemExistente == null)
        {
            Pecas.Add(OrdemDeServicoPeca.Criar(Id, peca.Id, quantidade, peca.Preco));
        }
        else
        {
            itemExistente.SomarQuantidade(quantidade, peca.Preco);
        }

        RecalcularTotal();

        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.PecaAdicionada,
            Status,
            Status,
            $"Peca adicionada na abertura: {peca.Nome}. Quantidade: {quantidade}.");
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
            TipoEventoOrdemServico.PecaRemovida,
            Status,
            Status,
            $"Peca removida do orcamento: {nomePeca}.");
    }

    public void AlterarStatus(StatusOrdemDeServico novoStatus, DateTime? dataConclusao = null)
    {
        if (!ValidarTransicaoDeStatus(Status, novoStatus))
        {
            throw new InvalidOperationException($"Nao e possivel transicionar de {Status} para {novoStatus}");
        }

        Status = novoStatus;

        if (novoStatus == StatusOrdemDeServico.Entregue)
        {
            DataConclusao = dataConclusao
                ?? throw new InvalidOperationException("Data de conclusao e obrigatoria para entregar a ordem de servico.");
        }
    }

    public void FinalizarDiagnostico(DateTime dataOrcamentoEnviado)
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
        OrcamentoEnviadoEm = dataOrcamentoEnviado;
    }

    public OrdemDeServicoEventoDominio FinalizarDiagnosticoComEvento(DateTime dataOrcamentoEnviado)
    {
        var statusAnterior = Status;
        FinalizarDiagnostico(dataOrcamentoEnviado);
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

        Status = StatusOrdemDeServico.EmExecucao;
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

    public void FinalizarServico(DateTime dataFinalizacao)
    {
        if (Status != StatusOrdemDeServico.EmExecucao)
        {
            throw new InvalidOperationException("So e possivel finalizar o servico quando a ordem estiver em execucao.");
        }

        DataFinalizacao = dataFinalizacao;
        AlterarStatus(StatusOrdemDeServico.Finalizada);
    }

    public OrdemDeServicoEventoDominio FinalizarServicoComEvento(DateTime dataFinalizacao)
    {
        var statusAnterior = Status;
        FinalizarServico(dataFinalizacao);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.ServicoFinalizado,
            statusAnterior,
            Status,
            "Servico finalizado.");
    }

    public void RegistrarPagamento(DateTime dataPagamento)
    {
        if (Status != StatusOrdemDeServico.Finalizada)
        {
            throw new InvalidOperationException("So e possivel registrar pagamento quando a ordem estiver finalizada.");
        }

        if (!DataFinalizacao.HasValue)
        {
            throw new InvalidOperationException("Nao e possivel registrar pagamento antes da finalizacao do servico.");
        }

        if (DataPagamento.HasValue)
        {
            throw new InvalidOperationException("Pagamento ja foi registrado para esta ordem de servico.");
        }

        DataPagamento = dataPagamento;
    }

    public OrdemDeServicoEventoDominio RegistrarPagamentoComEvento(DateTime dataPagamento)
    {
        RegistrarPagamento(dataPagamento);
        return new OrdemDeServicoEventoDominio(
            TipoEventoOrdemServico.PagamentoRegistrado,
            Status,
            Status,
            "Pagamento registrado para a ordem de servico.");
    }

    public void Entregar(DateTime dataConclusao)
    {
        if (Status == StatusOrdemDeServico.Entregue)
        {
            throw new InvalidOperationException("Veiculo ja foi entregue para esta ordem de servico.");
        }

        if (Status != StatusOrdemDeServico.Finalizada)
        {
            throw new InvalidOperationException("So e possivel entregar quando a ordem estiver finalizada.");
        }

        if (!DataPagamento.HasValue)
        {
            throw new InvalidOperationException("So e possivel entregar apos o pagamento ser registrado.");
        }

        AlterarStatus(StatusOrdemDeServico.Entregue, dataConclusao);
    }

    public OrdemDeServicoEventoDominio EntregarComEvento(DateTime dataConclusao)
    {
        var statusAnterior = Status;
        Entregar(dataConclusao);
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

    private static void ValidarDadosBase(
        string numero,
        string codigoAcompanhamento,
        string tokenAcompanhamentoHash,
        int clienteId,
        int veiculoId,
        string descricaoSolicitacao)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            throw new ArgumentException("Numero da ordem de servico e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(codigoAcompanhamento))
        {
            throw new ArgumentException("Codigo de acompanhamento da ordem de servico e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(tokenAcompanhamentoHash))
        {
            throw new ArgumentException("Token de acompanhamento da ordem de servico e obrigatorio.");
        }

        if (clienteId <= 0)
        {
            throw new ArgumentException("Cliente da ordem de servico e obrigatorio.");
        }

        if (veiculoId <= 0)
        {
            throw new ArgumentException("Veiculo da ordem de servico e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(descricaoSolicitacao))
        {
            throw new ArgumentException("Descricao da solicitacao da ordem de servico e obrigatoria.");
        }
    }
}

public sealed record OrdemDeServicoEventoDominio(
    TipoEventoOrdemServico TipoEvento,
    StatusOrdemDeServico? StatusAnterior,
    StatusOrdemDeServico? StatusNovo,
    string Descricao);

public class OrdemDeServicoServico
{
    private OrdemDeServicoServico()
    {
    }

    public int OrdemDeServicoId { get; private set; }
    public int ServicoId { get; private set; }
    public decimal Preco { get; private set; }
    public int TempoEstimado { get; private set; }

    public OrdemDeServico? OrdemDeServico { get; private set; }
    public Servico? Servico { get; private set; }

    public static OrdemDeServicoServico Criar(int ordemDeServicoId, int servicoId, decimal preco, int tempoEstimado)
    {
        return new OrdemDeServicoServico
        {
            OrdemDeServicoId = ordemDeServicoId,
            ServicoId = servicoId,
            Preco = preco,
            TempoEstimado = tempoEstimado
        };
    }
}

public class OrdemDeServicoPeca
{
    private OrdemDeServicoPeca()
    {
    }

    public int OrdemDeServicoId { get; private set; }
    public int PecaId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal Preco { get; private set; }

    public OrdemDeServico? OrdemDeServico { get; private set; }
    public Peca? Peca { get; private set; }

    public static OrdemDeServicoPeca Criar(int ordemDeServicoId, int pecaId, int quantidade, decimal preco)
    {
        return new OrdemDeServicoPeca
        {
            OrdemDeServicoId = ordemDeServicoId,
            PecaId = pecaId,
            Quantidade = quantidade,
            Preco = preco
        };
    }

    public void SomarQuantidade(int quantidade, decimal preco)
    {
        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade da peca deve ser maior que zero.");
        }

        Quantidade += quantidade;
        Preco = preco;
    }
}
