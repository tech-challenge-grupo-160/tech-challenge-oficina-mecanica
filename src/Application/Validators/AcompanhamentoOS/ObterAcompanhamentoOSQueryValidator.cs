using Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.AcompanhamentoOS;

public sealed class ObterAcompanhamentoOSQueryValidator : AbstractValidator<ObterAcompanhamentoOSQuery>
{
    public ObterAcompanhamentoOSQueryValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Codigo de acompanhamento e obrigatorio.");
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token de acompanhamento e obrigatorio.");
    }
}
