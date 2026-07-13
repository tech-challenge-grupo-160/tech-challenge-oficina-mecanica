using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class AvancarStatusOrdemDeServicoCommandValidator : AbstractValidator<AvancarStatusOrdemDeServicoCommand>
{
    public AvancarStatusOrdemDeServicoCommandValidator()
    {
        RuleFor(x => x.Numero).NotEmpty().WithMessage("Numero da ordem de servico e obrigatorio.");
    }
}
