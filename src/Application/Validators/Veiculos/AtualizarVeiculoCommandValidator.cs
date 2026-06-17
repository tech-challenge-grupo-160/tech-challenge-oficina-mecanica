using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public sealed class AtualizarVeiculoCommandValidator : AbstractValidator<AtualizarVeiculoCommand>
{
    public AtualizarVeiculoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id do veiculo deve ser maior que zero.");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("Marca e obrigatoria.")
            .MaximumLength(100);

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("Modelo e obrigatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Ano)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage("Ano do veiculo invalido.");
    }
}
