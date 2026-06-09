using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public sealed class CriarVeiculoParaClienteCommandValidator : AbstractValidator<CriarVeiculoParaClienteCommand>
{
    public CriarVeiculoParaClienteCommandValidator()
    {
        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CPF/CNPJ do proprietario e obrigatorio.")
            .Must(Documento.IsValid).WithMessage("CPF/CNPJ invalido.");

        RuleFor(x => x.Placa)
            .NotEmpty().WithMessage("Placa e obrigatoria.")
            .Must(PlacaVeiculo.IsValid).WithMessage("Placa invalida.");

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
