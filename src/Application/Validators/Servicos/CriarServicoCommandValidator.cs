using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Servicos;

public sealed class CriarServicoCommandValidator : AbstractValidator<CriarServicoCommand>
{
    public CriarServicoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio.")
            .MaximumLength(150);

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descricao e obrigatoria.")
            .MaximumLength(500);

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("Preco deve ser maior que zero.");

        RuleFor(x => x.TempoEstimado)
            .GreaterThan(0).WithMessage("Tempo estimado deve ser maior que zero.");
    }
}
