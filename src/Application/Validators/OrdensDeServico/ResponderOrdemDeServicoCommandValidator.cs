using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class ResponderOrdemDeServicoCommandValidator : AbstractValidator<ResponderOrdemDeServicoCommand>
{
    public ResponderOrdemDeServicoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
        RuleFor(x => x.MotivoRecusa)
            .MaximumLength(1000)
            .WithMessage("Motivo da recusa deve ter no maximo 1000 caracteres.");
    }
}
