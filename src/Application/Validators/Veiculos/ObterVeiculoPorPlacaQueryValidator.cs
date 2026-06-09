using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public sealed class ObterVeiculoPorPlacaQueryValidator : AbstractValidator<ObterVeiculoPorPlacaQuery>
{
    public ObterVeiculoPorPlacaQueryValidator()
    {
        RuleFor(x => x.Placa)
            .NotEmpty().WithMessage("Placa e obrigatoria.")
            .Must(PlacaVeiculo.IsValid).WithMessage("Placa invalida.");
    }
}
