using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Pecas;

public sealed class ObterPecaPorIdQueryValidator : AbstractValidator<ObterPecaPorIdQuery>
{
    public ObterPecaPorIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id da peca deve ser maior que zero.");
    }
}
