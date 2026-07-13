using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class ObterResumoMonitoramentoOrdensDeServicoQueryValidator : AbstractValidator<ObterResumoMonitoramentoOrdensDeServicoQuery>
{
    public ObterResumoMonitoramentoOrdensDeServicoQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("page deve ser maior que zero.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("pageSize deve ser maior que zero.")
            .LessThanOrEqualTo(100).WithMessage("pageSize deve ser menor ou igual a 100.");
    }
}

