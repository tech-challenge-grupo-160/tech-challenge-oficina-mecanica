using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class AdicionarPecaAOrdemCommandValidator : AbstractValidator<AdicionarPecaAOrdemCommand>
{
    public AdicionarPecaAOrdemCommandValidator()
    {
        RuleFor(x => x.OrdemDeServicoId).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
        RuleFor(x => x.PecaId).GreaterThan(0).WithMessage("PecaId deve ser maior que zero.");
        RuleFor(x => x.Quantidade).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
    }
}

