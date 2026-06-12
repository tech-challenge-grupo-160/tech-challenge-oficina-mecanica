using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class CancelarOrdemDeServicoCommandValidator : AbstractValidator<CancelarOrdemDeServicoCommand>
{
    public CancelarOrdemDeServicoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
        RuleFor(x => x.MotivoCancelamento)
            .NotEmpty().WithMessage("Motivo do cancelamento e obrigatorio.")
            .MaximumLength(1000).WithMessage("Motivo do cancelamento deve ter no maximo 1000 caracteres.");
    }
}

