using Fiap.TechChallenge.OficinaMecanica.Application.Results.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Mappers;

public static class AcompanhamentoOSMapper
{
    public static AcompanhamentoOrdemDeServicoResult ToAcompanhamentoResult(this OrdemDeServico ordem, DateTime dataUltimaAtualizacao)
    {
        return new AcompanhamentoOrdemDeServicoResult
        {
            Numero = ordem.Numero,
            CodigoAcompanhamento = ordem.CodigoAcompanhamento,
            Status = ordem.Status.ToString(),
            DataAbertura = ordem.DataAbertura,
            DataUltimaAtualizacao = dataUltimaAtualizacao,
            OrcamentoEnviadoEm = ordem.OrcamentoEnviadoEm,
            DataFinalizacao = ordem.DataFinalizacao,
            DataPagamento = ordem.DataPagamento,
            DataConclusao = ordem.DataConclusao
        };
    }
}
