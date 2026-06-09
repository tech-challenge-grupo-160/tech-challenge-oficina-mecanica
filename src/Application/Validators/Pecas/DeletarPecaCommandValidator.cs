using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Pecas;

public sealed class DeletarPecaCommandValidator : AbstractValidator<DeletarPecaCommand>
{
    public DeletarPecaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id da peca deve ser maior que zero.");
    }
}
