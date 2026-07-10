using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Clientes;

public sealed class ListarClientesQueryValidator : AbstractValidator<ListarClientesQuery>
{
    public ListarClientesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Pagina deve ser maior que zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Tamanho da pagina deve ser maior que zero.")
            .LessThanOrEqualTo(100).WithMessage("Tamanho da pagina deve ser menor ou igual a 100.");
    }
}
