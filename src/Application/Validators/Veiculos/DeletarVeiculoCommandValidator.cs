using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public sealed class DeletarVeiculoCommandValidator : AbstractValidator<DeletarVeiculoCommand>
{
    public DeletarVeiculoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id do veiculo deve ser maior que zero.");
    }
}
