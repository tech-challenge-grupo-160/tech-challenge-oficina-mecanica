using FluentValidation;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public class CancelarOrdemDeServicoDtoValidator : AbstractValidator<CancelarOrdemDeServicoDto>
{
    public CancelarOrdemDeServicoDtoValidator()
    {
        RuleFor(x => x.MotivoCancelamento)
            .NotEmpty()
            .WithMessage("Motivo do cancelamento e obrigatorio.")
            .MaximumLength(1000)
            .WithMessage("Motivo do cancelamento deve ter no maximo 1000 caracteres.");
    }
}
