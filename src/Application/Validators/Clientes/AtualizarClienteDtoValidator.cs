using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Clientes;

public class AtualizarClienteDtoValidator : AbstractValidator<AtualizarClienteDto>
{
    public AtualizarClienteDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail inválido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .Must(TelefoneHelper.IsValid).WithMessage("Telefone deve conter 10 ou 11 dígitos.");
    }
}
