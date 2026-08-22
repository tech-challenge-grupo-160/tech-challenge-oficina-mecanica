using Fiap.TechChallenge.OficinaMecanica.API.Requests.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.AcompanhamentoOS;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class AcompanhamentoOSApiMapper
{
    public static ObterAcompanhamentoOSQuery ToQuery(this ObterAcompanhamentoOSRequest request)
    {
        return new ObterAcompanhamentoOSQuery
        {
            CodigoAcompanhamento = request.CodigoAcompanhamento
        };
    }

    public static AcompanhamentoOrdemDeServicoResponse ToResponse(this AcompanhamentoOrdemDeServicoResult result)
    {
        return new AcompanhamentoOrdemDeServicoResponse
        {
            Numero = result.Numero,
            CodigoAcompanhamento = result.CodigoAcompanhamento,
            Status = result.Status,
            DataAbertura = result.DataAbertura,
            DataUltimaAtualizacao = result.DataUltimaAtualizacao,
            OrcamentoEnviadoEm = result.OrcamentoEnviadoEm,
            DataFinalizacao = result.DataFinalizacao,
            DataPagamento = result.DataPagamento,
            DataConclusao = result.DataConclusao
        };
    }
}
