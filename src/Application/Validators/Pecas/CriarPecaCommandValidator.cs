using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Pecas;

public sealed class CriarPecaCommandValidator : AbstractValidator<CriarPecaCommand>
{
    public CriarPecaCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio.")
            .MaximumLength(150);

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("Marca e obrigatoria.")
            .MaximumLength(100);

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("Modelo e obrigatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("Preco deve ser maior que zero.");

        RuleFor(x => x.QuantidadeEstoque)
            .GreaterThanOrEqualTo(0).WithMessage("Quantidade em estoque nao pode ser negativa.");
    }
}
