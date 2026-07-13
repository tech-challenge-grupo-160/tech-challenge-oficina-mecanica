using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Clientes;

public sealed class ObterClientePorDocumentoQueryValidator : AbstractValidator<ObterClientePorDocumentoQuery>
{
    public ObterClientePorDocumentoQueryValidator()
    {
        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CPF/CNPJ e obrigatorio.")
            .Must(Documento.IsValid).WithMessage("CPF/CNPJ invalido.");
    }
}
