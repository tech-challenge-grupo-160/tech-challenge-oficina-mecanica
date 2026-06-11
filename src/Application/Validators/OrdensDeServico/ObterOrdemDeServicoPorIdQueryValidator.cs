using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class ObterOrdemDeServicoPorIdQueryValidator : AbstractValidator<ObterOrdemDeServicoPorIdQuery>
{
    public ObterOrdemDeServicoPorIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
    }
}

