namespace Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

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
