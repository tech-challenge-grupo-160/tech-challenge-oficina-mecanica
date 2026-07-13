using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public sealed class ObterVeiculoPorIdQueryValidator : AbstractValidator<ObterVeiculoPorIdQuery>
{
    public ObterVeiculoPorIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id do veiculo deve ser maior que zero.");
    }
}
