using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class RemoverPecaDaOrdemCommandValidator : AbstractValidator<RemoverPecaDaOrdemCommand>
{
    public RemoverPecaDaOrdemCommandValidator()
    {
        RuleFor(x => x.OrdemDeServicoId).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
        RuleFor(x => x.PecaId).GreaterThan(0).WithMessage("PecaId deve ser maior que zero.");
    }
}

