using FluentValidation;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public class CriarOrdemDeServicoDtoValidator : AbstractValidator<CriarOrdemDeServicoDto>
{
    public CriarOrdemDeServicoDtoValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("ClienteId deve ser maior que zero.");

        RuleFor(x => x.VeiculoId)
            .GreaterThan(0)
            .WithMessage("VeiculoId deve ser maior que zero.");

        RuleFor(x => x.DescricaoSolicitacao)
            .NotEmpty()
            .WithMessage("Descricao da solicitacao e obrigatoria.")
            .MaximumLength(1000)
            .WithMessage("Descricao da solicitacao deve ter no maximo 1000 caracteres.");

        RuleFor(x => x.ObservacoesRecepcao)
            .MaximumLength(1000)
            .WithMessage("Observacoes da recepcao devem ter no maximo 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.ObservacoesRecepcao));
    }
}
