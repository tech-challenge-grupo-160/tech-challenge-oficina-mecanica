using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;

public sealed class OrdemDeServicoAcompanhamentoService
{
    private readonly IOrdemDeServicoRepository _ordemRepository;

    public OrdemDeServicoAcompanhamentoService(IOrdemDeServicoRepository ordemRepository)
    {
        _ordemRepository = ordemRepository;
    }

    public static string GerarNumeroTemporario()
    {
        return $"TMP-{Guid.NewGuid():N}";
    }

    public static string GerarNumeroOrdem(int id, DateTime dataAbertura)
    {
        return $"OS-{dataAbertura:yyyyMMdd}-{id}";
    }

    public static string MontarEndpointAcompanhamento(string codigoAcompanhamento)
    {
        return $"/api/v1/acompanhamento-os/{codigoAcompanhamento}";
    }

    public async Task<(string Codigo, string Token, string TokenHash)> GerarCredenciaisAsync(CancellationToken cancellationToken)
    {
        for (var tentativa = 0; tentativa < 5; tentativa++)
        {
            var codigo = $"AC-{StringHelper.GenerateSecureHexToken(8)}";
            var existente = await _ordemRepository.ObterPorCodigoAcompanhamentoAsync(codigo, cancellationToken);
            if (existente is not null)
            {
                continue;
            }

            var token = StringHelper.GenerateSecureHexToken(32);
            return (codigo, token, StringHelper.ToSha256Hash(token));
        }

        throw new InvalidOperationException("Nao foi possivel gerar credenciais de acompanhamento unicas.");
    }
}
