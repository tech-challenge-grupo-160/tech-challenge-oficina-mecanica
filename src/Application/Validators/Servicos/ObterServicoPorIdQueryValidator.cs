using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Servicos;

public sealed class ObterServicoPorIdQueryValidator : AbstractValidator<ObterServicoPorIdQuery>
{
    public ObterServicoPorIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id do servico deve ser maior que zero.");
    }
}
