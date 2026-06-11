using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Mappers;

public static class OrdemDeServicoMapper
{
    public static OrdemServicoHistoricoDto ToDto(this OrdemServicoHistorico historico)
    {
        return new OrdemServicoHistoricoDto
        {
            Id = historico.Id,
            OrdemDeServicoId = historico.OrdemDeServicoId,
            UsuarioId = historico.UsuarioId,
            UsuarioNome = historico.UsuarioNome,
            StatusAnterior = historico.StatusAnterior?.ToString(),
            StatusNovo = historico.StatusNovo?.ToString(),
            TipoEvento = historico.TipoEvento.ToString(),
            Descricao = historico.Descricao,
            DataEvento = historico.DataEvento
        };
    }

    public static NotificacaoClienteDto ToDto(this NotificacaoCliente notificacao)
    {
        return new NotificacaoClienteDto
        {
            Id = notificacao.Id,
            OrdemDeServicoId = notificacao.OrdemDeServicoId,
            Canal = notificacao.Canal.ToString(),
            TipoNotificacao = notificacao.TipoNotificacao.ToString(),
            Mensagem = notificacao.Mensagem,
            Recebida = notificacao.Recebida,
            DataNotificacao = notificacao.DataNotificacao
        };
    }

    public static OrdemDeServicoDto ToDto(this OrdemDeServico ordem)
    {
        return new OrdemDeServicoDto
        {
            Id = ordem.Id,
            Numero = ordem.Numero,
            CodigoAcompanhamento = ordem.CodigoAcompanhamento,
            UrlAcompanhamento = OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordem.CodigoAcompanhamento),
            TokenAcompanhamento = null,
            ClienteId = ordem.ClienteId,
            VeiculoId = ordem.VeiculoId,
            DescricaoSolicitacao = ordem.DescricaoSolicitacao,
            ObservacoesRecepcao = ordem.ObservacoesRecepcao,
            MotivoCancelamento = ordem.MotivoCancelamento,
            OrcamentoEnviadoEm = ordem.OrcamentoEnviadoEm,
            DataFinalizacao = ordem.DataFinalizacao,
            DataPagamento = ordem.DataPagamento,
            Status = ordem.Status.ToString(),
            DataAbertura = ordem.DataAbertura,
            DataConclusao = ordem.DataConclusao,
            ValorTotal = ordem.ValorTotal,
            Servicos = ordem.Servicos.Select(s => new OrdemDeServicoServicoDto
            {
                ServicoId = s.ServicoId,
                Preco = s.Preco,
                TempoEstimado = s.TempoEstimado
            }).ToList(),
            Pecas = ordem.Pecas.Select(p => new OrdemDeServicoPecaDto
            {
                PecaId = p.PecaId,
                Quantidade = p.Quantidade,
                Preco = p.Preco
            }).ToList()
        };
    }

    public static MovimentacaoEstoqueDto ToDto(this MovimentacaoEstoque movimentacao)
    {
        return new MovimentacaoEstoqueDto
        {
            Id = movimentacao.Id,
            PecaId = movimentacao.PecaId,
            OrdemDeServicoId = movimentacao.OrdemDeServicoId,
            PedidoCompraId = movimentacao.PedidoCompraId,
            NomePeca = movimentacao.Peca?.Nome ?? string.Empty,
            TipoMovimentacao = movimentacao.TipoMovimentacao.ToString(),
            Quantidade = movimentacao.Quantidade,
            QuantidadeAnterior = movimentacao.QuantidadeAnterior,
            QuantidadePosterior = movimentacao.QuantidadePosterior,
            Descricao = movimentacao.Descricao,
            DataMovimentacao = movimentacao.DataMovimentacao
        };
    }

    public static MonitoramentoOrdemDeServicoDto ToMonitoramentoDto(this OrdemDeServico ordem, DateTime agora)
    {
        var dataReferencia = ordem.DataFinalizacao ?? agora;
        var tempoDecorrido = dataReferencia - ordem.DataAbertura;
        var tempoFinalizacao = ordem.DataFinalizacao.HasValue
            ? ordem.DataFinalizacao.Value - ordem.DataAbertura
            : (TimeSpan?)null;

        return new MonitoramentoOrdemDeServicoDto
        {
            Id = ordem.Id,
            Numero = ordem.Numero,
            Status = ordem.Status.ToString(),
            DataAbertura = ordem.DataAbertura,
            DataFinalizacao = ordem.DataFinalizacao,
            EstaFinalizada = ordem.DataFinalizacao.HasValue,
            TempoDecorridoMinutos = Math.Max(0, (int)Math.Round(tempoDecorrido.TotalMinutes)),
            TempoDecorridoHoras = Math.Max(0, Math.Round(tempoDecorrido.TotalHours, 2)),
            TempoFinalizacaoMinutos = tempoFinalizacao.HasValue
                ? Math.Max(0, (int)Math.Round(tempoFinalizacao.Value.TotalMinutes))
                : null,
            TempoFinalizacaoHoras = tempoFinalizacao.HasValue
                ? Math.Max(0, Math.Round(tempoFinalizacao.Value.TotalHours, 2))
                : null
        };
    }

    public static EstimativaTempoOrdemDeServicoDto ToEstimativaTempoDto(this OrdemDeServico ordem)
    {
        var servicos = ordem.Servicos
            .OrderBy(x => x.ServicoId)
            .Select(x => new EstimativaTempoServicoDto
            {
                ServicoId = x.ServicoId,
                TempoEstimadoMinutos = x.TempoEstimado,
                TempoEstimadoHoras = Math.Round(x.TempoEstimado / 60d, 2)
            })
            .ToList();

        var totalMinutos = servicos.Sum(x => x.TempoEstimadoMinutos);

        return new EstimativaTempoOrdemDeServicoDto
        {
            OrdemDeServicoId = ordem.Id,
            Numero = ordem.Numero,
            Status = ordem.Status.ToString(),
            TotalServicos = servicos.Count,
            TempoEstimadoMinutos = totalMinutos,
            TempoEstimadoHoras = Math.Round(totalMinutos / 60d, 2),
            Servicos = servicos
        };
    }
}
