using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class AdicionarServicoAOrdemCommandValidator : AbstractValidator<AdicionarServicoAOrdemCommand>
{
    public AdicionarServicoAOrdemCommandValidator()
    {
        RuleFor(x => x.OrdemDeServicoId).GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
        RuleFor(x => x.ServicoId).GreaterThan(0).WithMessage("ServicoId deve ser maior que zero.");
    }
}

