using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public class CriarVeiculoDtoValidator : AbstractValidator<CriarVeiculoDto>
{
    public CriarVeiculoDtoValidator()
    {
        RuleFor(x => x.Placa)
            .NotEmpty().WithMessage("Placa e obrigatoria.")
            .Must(PlacaHelper.IsValid).WithMessage("Placa invalida.");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("Marca e obrigatoria.")
            .MaximumLength(100);

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("Modelo e obrigatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Ano)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage("Ano do veiculo invalido.");

        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CPF/CNPJ do proprietario e obrigatorio.")
            .Must(x =>
            {
                var tamanho = StringHelper.OnlyDigits(x).Length;
                return tamanho is 11 or 14;
            }).WithMessage("CPF/CNPJ deve conter 11 ou 14 digitos.")
            .Must(DocumentoHelper.ValidarCpf).When(x => StringHelper.OnlyDigits(x.CpfCnpj).Length == 11)
            .WithMessage("CPF invalido.")
            .Must(DocumentoHelper.ValidarCnpj).When(x => StringHelper.OnlyDigits(x.CpfCnpj).Length == 14)
            .WithMessage("CNPJ invalido.");
    }
}
