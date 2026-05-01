using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public class AtualizarVeiculoDtoValidator : AbstractValidator<AtualizarVeiculoDto>
{
    public AtualizarVeiculoDtoValidator()
    {
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
