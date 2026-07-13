using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public sealed class ListarVeiculosPorClienteQueryValidator : AbstractValidator<ListarVeiculosPorClienteQuery>
{
    public ListarVeiculosPorClienteQueryValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0).WithMessage("Id do cliente deve ser maior que zero.");
    }
}
