using Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.AcompanhamentoOS;

public sealed class ObterAcompanhamentoOSQueryValidator : AbstractValidator<ObterAcompanhamentoOSQuery>
{
    public ObterAcompanhamentoOSQueryValidator()
    {
        RuleFor(x => x.CodigoAcompanhamento)
            .NotEmpty().WithMessage("Codigo de acompanhamento e obrigatorio.");
    }
}
