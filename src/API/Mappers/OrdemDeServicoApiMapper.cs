using Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class OrdemDeServicoApiMapper
{
    public static CriarOrdemDeServicoCommand ToCommand(this CriarOrdemDeServicoRequest request)
    {
        return new CriarOrdemDeServicoCommand
        {
            ClienteId = request.ClienteId,
            VeiculoId = request.VeiculoId,
            DescricaoSolicitacao = request.DescricaoSolicitacao,
            ObservacoesRecepcao = request.ObservacoesRecepcao,
            Servicos = (request.Servicos ?? Array.Empty<CriarOrdemDeServicoServicoRequest>()).Select(servico => new CriarOrdemDeServicoServicoCommand
            {
                ServicoId = servico.ServicoId
            }).ToArray(),
            Pecas = (request.Pecas ?? Array.Empty<CriarOrdemDeServicoPecaRequest>()).Select(peca => new CriarOrdemDeServicoPecaCommand
            {
                PecaId = peca.PecaId,
                Quantidade = peca.Quantidade
            }).ToArray()
        };
    }

    public static CancelarOrdemDeServicoCommand ToCommand(this CancelarOrdemDeServicoRequest request, int id)
    {
        return new CancelarOrdemDeServicoCommand
        {
            Id = id,
            MotivoCancelamento = request.MotivoCancelamento
        };
    }

    public static ResponderOrdemDeServicoCommand ToCommand(
        this ResponderOrdemDeServicoRequest request,
        int id,
        string trackingToken)
    {
        return new ResponderOrdemDeServicoCommand
        {
            Id = id,
            TrackingToken = trackingToken,
            Aprovado = request.Aprovado,
            MotivoRecusa = request.MotivoRecusa
        };
    }

    public static AdicionarServicoAOrdemCommand ToCommand(this AdicionarServicoAOrdemRequest request, int id)
    {
        return new AdicionarServicoAOrdemCommand
        {
            OrdemDeServicoId = id,
            ServicoId = request.ServicoId
        };
    }

    public static AdicionarPecaAOrdemCommand ToCommand(this AdicionarPecaAOrdemRequest request, int id)
    {
        return new AdicionarPecaAOrdemCommand
        {
            OrdemDeServicoId = id,
            PecaId = request.PecaId,
            Quantidade = request.Quantidade
        };
    }

    public static ObterOrdemDeServicoPorIdQuery ToOrdemDeServicoQueryById(this int id)
    {
        return new ObterOrdemDeServicoPorIdQuery { Id = id };
    }

    public static ObterHistoricoOrdemDeServicoQuery ToHistoricoOrdemDeServicoQuery(this int id)
    {
        return new ObterHistoricoOrdemDeServicoQuery { Id = id };
    }

    public static ObterNotificacoesOrdemDeServicoQuery ToNotificacoesOrdemDeServicoQuery(this int id)
    {
        return new ObterNotificacoesOrdemDeServicoQuery { Id = id };
    }

    public static ObterMovimentacoesEstoqueOrdemDeServicoQuery ToMovimentacoesEstoqueOrdemDeServicoQuery(this int id)
    {
        return new ObterMovimentacoesEstoqueOrdemDeServicoQuery { Id = id };
    }

    public static ObterMonitoramentoOrdemDeServicoQuery ToMonitoramentoOrdemDeServicoQuery(this int id)
    {
        return new ObterMonitoramentoOrdemDeServicoQuery { Id = id };
    }

    public static ObterEstimativaTempoOrdemDeServicoQuery ToEstimativaTempoOrdemDeServicoQuery(this int id)
    {
        return new ObterEstimativaTempoOrdemDeServicoQuery { Id = id };
    }

    public static IniciarDiagnosticoCommand ToIniciarDiagnosticoCommand(this int id)
    {
        return new IniciarDiagnosticoCommand { Id = id };
    }

    public static FinalizarDiagnosticoCommand ToFinalizarDiagnosticoCommand(this int id)
    {
        return new FinalizarDiagnosticoCommand { Id = id };
    }

    public static AprovarOrdemDeServicoCommand ToAprovarOrdemDeServicoCommand(this int id)
    {
        return new AprovarOrdemDeServicoCommand { Id = id };
    }

    public static LiberarExecucaoCommand ToLiberarExecucaoCommand(this int id)
    {
        return new LiberarExecucaoCommand { Id = id };
    }

    public static FinalizarOrdemDeServicoCommand ToFinalizarOrdemDeServicoCommand(this int id)
    {
        return new FinalizarOrdemDeServicoCommand { Id = id };
    }

    public static RegistrarPagamentoCommand ToRegistrarPagamentoCommand(this int id)
    {
        return new RegistrarPagamentoCommand { Id = id };
    }

    public static EntregarOrdemDeServicoCommand ToEntregarOrdemDeServicoCommand(this int id)
    {
        return new EntregarOrdemDeServicoCommand { Id = id };
    }

    public static AvancarStatusOrdemDeServicoCommand ToAvancarStatusOrdemDeServicoCommand(this string numero)
    {
        return new AvancarStatusOrdemDeServicoCommand { Numero = numero };
    }

    public static RemoverServicoDaOrdemCommand ToRemoverServicoDaOrdemCommand(this int id, int servicoId)
    {
        return new RemoverServicoDaOrdemCommand
        {
            OrdemDeServicoId = id,
            ServicoId = servicoId
        };
    }

    public static RemoverPecaDaOrdemCommand ToRemoverPecaDaOrdemCommand(this int id, int pecaId)
    {
        return new RemoverPecaDaOrdemCommand
        {
            OrdemDeServicoId = id,
            PecaId = pecaId
        };
    }

    public static OrdemDeServicoResponse ToResponse(this OrdemDeServicoResult result)
    {
        return new OrdemDeServicoResponse
        {
            Id = result.Id,
            Numero = result.Numero,
            CodigoAcompanhamento = result.CodigoAcompanhamento,
            UrlAcompanhamento = result.UrlAcompanhamento,
            TokenAcompanhamento = result.TokenAcompanhamento,
            ClienteId = result.ClienteId,
            VeiculoId = result.VeiculoId,
            DescricaoSolicitacao = result.DescricaoSolicitacao,
            ObservacoesRecepcao = result.ObservacoesRecepcao,
            MotivoCancelamento = result.MotivoCancelamento,
            OrcamentoEnviadoEm = result.OrcamentoEnviadoEm,
            DataFinalizacao = result.DataFinalizacao,
            DataPagamento = result.DataPagamento,
            Status = result.Status,
            DataAbertura = result.DataAbertura,
            DataConclusao = result.DataConclusao,
            ValorTotal = result.ValorTotal,
            Servicos = result.Servicos.Select(servico => servico.ToResponse()).ToList(),
            Pecas = result.Pecas.Select(peca => peca.ToResponse()).ToList()
        };
    }

    public static OrdemDeServicoServicoResponse ToResponse(this OrdemDeServicoServicoResult result)
    {
        return new OrdemDeServicoServicoResponse
        {
            ServicoId = result.ServicoId,
            Preco = result.Preco,
            TempoEstimado = result.TempoEstimado
        };
    }

    public static OrdemDeServicoPecaResponse ToResponse(this OrdemDeServicoPecaResult result)
    {
        return new OrdemDeServicoPecaResponse
        {
            PecaId = result.PecaId,
            Quantidade = result.Quantidade,
            Preco = result.Preco
        };
    }

    public static OrdemServicoHistoricoResponse ToResponse(this OrdemServicoHistoricoResult result)
    {
        return new OrdemServicoHistoricoResponse
        {
            Id = result.Id,
            OrdemDeServicoId = result.OrdemDeServicoId,
            UsuarioId = result.UsuarioId,
            UsuarioNome = result.UsuarioNome,
            StatusAnterior = result.StatusAnterior,
            StatusNovo = result.StatusNovo,
            TipoEvento = result.TipoEvento,
            Descricao = result.Descricao,
            DataEvento = result.DataEvento
        };
    }

    public static NotificacaoClienteResponse ToResponse(this NotificacaoClienteResult result)
    {
        return new NotificacaoClienteResponse
        {
            Id = result.Id,
            OrdemDeServicoId = result.OrdemDeServicoId,
            Canal = result.Canal,
            TipoNotificacao = result.TipoNotificacao,
            Mensagem = result.Mensagem,
            Recebida = result.Recebida,
            DataNotificacao = result.DataNotificacao
        };
    }

    public static MovimentacaoEstoqueResponse ToResponse(this MovimentacaoEstoqueResult result)
    {
        return new MovimentacaoEstoqueResponse
        {
            Id = result.Id,
            PecaId = result.PecaId,
            OrdemDeServicoId = result.OrdemDeServicoId,
            PedidoCompraId = result.PedidoCompraId,
            NomePeca = result.NomePeca,
            TipoMovimentacao = result.TipoMovimentacao,
            Quantidade = result.Quantidade,
            QuantidadeAnterior = result.QuantidadeAnterior,
            QuantidadePosterior = result.QuantidadePosterior,
            Descricao = result.Descricao,
            DataMovimentacao = result.DataMovimentacao
        };
    }

    public static MovimentacoesEstoquePorPecaResponse ToResponse(this MovimentacoesEstoquePorPecaResult result)
    {
        return new MovimentacoesEstoquePorPecaResponse
        {
            PecaId = result.PecaId,
            NomePeca = result.NomePeca,
            MarcaPeca = result.MarcaPeca,
            ModeloPeca = result.ModeloPeca,
            QuantidadeNaOrdem = result.QuantidadeNaOrdem,
            TotalMovimentacoes = result.TotalMovimentacoes,
            Movimentacoes = result.Movimentacoes.Select(movimentacao => movimentacao.ToResponse()).ToList()
        };
    }

    public static MonitoramentoOrdemDeServicoResponse ToResponse(this MonitoramentoOrdemDeServicoResult result)
    {
        return new MonitoramentoOrdemDeServicoResponse
        {
            Id = result.Id,
            Numero = result.Numero,
            Status = result.Status,
            DataAbertura = result.DataAbertura,
            DataFinalizacao = result.DataFinalizacao,
            EstaFinalizada = result.EstaFinalizada,
            TempoDecorridoMinutos = result.TempoDecorridoMinutos,
            TempoDecorridoHoras = result.TempoDecorridoHoras,
            TempoFinalizacaoMinutos = result.TempoFinalizacaoMinutos,
            TempoFinalizacaoHoras = result.TempoFinalizacaoHoras
        };
    }

    public static EstimativaTempoOrdemDeServicoResponse ToResponse(this EstimativaTempoOrdemDeServicoResult result)
    {
        return new EstimativaTempoOrdemDeServicoResponse
        {
            OrdemDeServicoId = result.OrdemDeServicoId,
            Numero = result.Numero,
            Status = result.Status,
            TotalServicos = result.TotalServicos,
            TempoEstimadoMinutos = result.TempoEstimadoMinutos,
            TempoEstimadoHoras = result.TempoEstimadoHoras,
            Servicos = result.Servicos.Select(servico => servico.ToResponse()).ToList()
        };
    }

    public static EstimativaTempoServicoResponse ToResponse(this EstimativaTempoServicoResult result)
    {
        return new EstimativaTempoServicoResponse
        {
            ServicoId = result.ServicoId,
            TempoEstimadoMinutos = result.TempoEstimadoMinutos,
            TempoEstimadoHoras = result.TempoEstimadoHoras
        };
    }

    public static ResumoMonitoramentoOrdensDeServicoResponse ToResponse(this ResumoMonitoramentoOrdensDeServicoResult result)
    {
        return new ResumoMonitoramentoOrdensDeServicoResponse
        {
            TotalOrdens = result.TotalOrdens,
            TotalOrdensAbertas = result.TotalOrdensAbertas,
            TotalOrdensFinalizadas = result.TotalOrdensFinalizadas,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TempoMedioFinalizacaoMinutos = result.TempoMedioFinalizacaoMinutos,
            TempoMedioFinalizacaoHoras = result.TempoMedioFinalizacaoHoras,
            Ordens = result.Ordens.Select(ordem => ordem.ToResponse()).ToList()
        };
    }

    public static IEnumerable<OrdemServicoHistoricoResponse> ToResponse(this IEnumerable<OrdemServicoHistoricoResult> results)
    {
        return results.Select(historico => historico.ToResponse()).ToArray();
    }

    public static IEnumerable<NotificacaoClienteResponse> ToResponse(this IEnumerable<NotificacaoClienteResult> results)
    {
        return results.Select(notificacao => notificacao.ToResponse()).ToArray();
    }

    public static IEnumerable<MovimentacoesEstoquePorPecaResponse> ToResponse(this IEnumerable<MovimentacoesEstoquePorPecaResult> results)
    {
        return results.Select(movimentacao => movimentacao.ToResponse()).ToArray();
    }

    public static PagedResponse<OrdemDeServicoResponse> ToResponse(this PagedResultDto<OrdemDeServicoResult> result)
    {
        return new PagedResponse<OrdemDeServicoResponse>
        {
            Items = result.Items.Select(ordem => ordem.ToResponse()).ToArray(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }
}
