using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class LiberarExecucaoCommandValidator : AbstractValidator<LiberarExecucaoCommand>
{
    public LiberarExecucaoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
    }
}

