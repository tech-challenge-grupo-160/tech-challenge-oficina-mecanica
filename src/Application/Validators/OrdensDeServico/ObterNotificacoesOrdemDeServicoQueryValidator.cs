using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class ObterNotificacoesOrdemDeServicoQueryValidator : AbstractValidator<ObterNotificacoesOrdemDeServicoQuery>
{
    public ObterNotificacoesOrdemDeServicoQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
    }
}

