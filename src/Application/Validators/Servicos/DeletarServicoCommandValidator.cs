using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Servicos;

public sealed class DeletarServicoCommandValidator : AbstractValidator<DeletarServicoCommand>
{
    public DeletarServicoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id do servico deve ser maior que zero.");
    }
}
