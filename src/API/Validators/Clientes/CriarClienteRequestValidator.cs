using Fiap.TechChallenge.OficinaMecanica.API.Requests.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.API.Validators.Clientes;

public sealed class CriarClienteRequestValidator : AbstractValidator<CriarClienteRequest>
{
    public CriarClienteRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio.")
            .MaximumLength(200);

        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CPF/CNPJ e obrigatorio.")
            .Must(Documento.IsValid).WithMessage("CPF/CNPJ invalido.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail e obrigatorio.");

        RuleFor(x => x.Email)
            .Must(Email.IsValid).WithMessage("E-mail invalido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone e obrigatorio.")
            .Must(Telefone.IsValid).WithMessage("Telefone deve conter 10 ou 11 digitos.");
    }
}
