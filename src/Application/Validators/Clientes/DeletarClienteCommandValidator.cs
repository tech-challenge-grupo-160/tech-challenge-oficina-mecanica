using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Clientes;

public sealed class DeletarClienteCommandValidator : AbstractValidator<DeletarClienteCommand>
{
    public DeletarClienteCommandValidator()
    {
        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CPF/CNPJ e obrigatorio.")
            .Must(Documento.IsValid).WithMessage("CPF/CNPJ invalido.");
    }
}
