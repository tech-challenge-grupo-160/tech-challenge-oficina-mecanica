using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;

public sealed class ListarVeiculosPorDocumentoClienteQueryValidator : AbstractValidator<ListarVeiculosPorDocumentoClienteQuery>
{
    public ListarVeiculosPorDocumentoClienteQueryValidator()
    {
        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CPF/CNPJ do proprietario e obrigatorio.")
            .Must(Documento.IsValid).WithMessage("CPF/CNPJ invalido.");
    }
}
