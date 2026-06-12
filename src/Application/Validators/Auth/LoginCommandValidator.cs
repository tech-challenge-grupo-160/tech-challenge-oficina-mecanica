using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Auth;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.Auth;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Usuario)
            .NotEmpty()
            .WithMessage("Usuario e obrigatorio.");

        RuleFor(x => x.Senha)
            .NotEmpty()
            .WithMessage("Senha e obrigatoria.");
    }
}
