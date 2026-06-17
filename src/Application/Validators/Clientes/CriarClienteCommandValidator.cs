using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Clientes;

public sealed class CriarClienteCommandValidator : AbstractValidator<CriarClienteCommand>
{
    public CriarClienteCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio.")
            .MaximumLength(255);

        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CPF/CNPJ e obrigatorio.")
            .Must(Documento.IsValid).WithMessage("CPF/CNPJ invalido.");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone e obrigatorio.")
            .Must(Telefone.IsValid).WithMessage("Telefone invalido.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail e obrigatorio.")
            .EmailAddress().WithMessage("E-mail invalido.")
            .MaximumLength(255);
    }
}
