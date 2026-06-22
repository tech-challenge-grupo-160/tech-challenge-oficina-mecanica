using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.DTOs;

public static class CriarOrdemDeServicoDtoMock
{
    public static CriarOrdemDeServicoDto Criar(
        int clienteId = 1000,
        int veiculoId = 1000,
        string descricaoSolicitacao = "Cliente relatou barulho ao frear.",
        string? observacoesRecepcao = "Problema ocorre em baixa velocidade.",
        int servicoId = 1000)
    {
        return new CriarOrdemDeServicoDto
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            DescricaoSolicitacao = descricaoSolicitacao,
            ObservacoesRecepcao = observacoesRecepcao,
            Servicos = new[]
            {
                new CriarOrdemDeServicoServicoDto { ServicoId = servicoId }
            },
            Pecas = Array.Empty<CriarOrdemDeServicoPecaDto>()
        };
    }
}
