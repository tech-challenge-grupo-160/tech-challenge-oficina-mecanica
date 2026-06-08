using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Clientes;

public sealed class ObterClientePorIdQueryValidator : AbstractValidator<ObterClientePorIdQuery>
{
    public ObterClientePorIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id do cliente deve ser maior que zero.");
    }
}
