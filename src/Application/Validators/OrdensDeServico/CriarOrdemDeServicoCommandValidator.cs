using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;

public sealed class CriarOrdemDeServicoCommandValidator : AbstractValidator<CriarOrdemDeServicoCommand>
{
    public CriarOrdemDeServicoCommandValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0).WithMessage("ClienteId deve ser maior que zero.");
        RuleFor(x => x.VeiculoId).GreaterThan(0).WithMessage("VeiculoId deve ser maior que zero.");
        RuleFor(x => x.DescricaoSolicitacao)
            .NotEmpty().WithMessage("Descricao da solicitacao e obrigatoria.")
            .MaximumLength(1000).WithMessage("Descricao da solicitacao deve ter no maximo 1000 caracteres.");
        RuleFor(x => x.ObservacoesRecepcao)
            .MaximumLength(1000).WithMessage("Observacoes da recepcao devem ter no maximo 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.ObservacoesRecepcao));
        RuleFor(x => x.Servicos)
            .NotEmpty().WithMessage("A ordem de servico deve possuir ao menos um servico.");
        RuleForEach(x => x.Servicos).ChildRules(servico =>
        {
            servico.RuleFor(x => x.ServicoId)
                .GreaterThan(0).WithMessage("ServicoId deve ser maior que zero.");
        });
        RuleFor(x => x.Servicos)
            .Must(servicos => servicos.Select(x => x.ServicoId).Distinct().Count() == servicos.Count)
            .WithMessage("Nao e permitido informar servicos duplicados.")
            .When(x => x.Servicos != null && x.Servicos.Count > 0);
        RuleForEach(x => x.Pecas).ChildRules(peca =>
        {
            peca.RuleFor(x => x.PecaId)
                .GreaterThan(0).WithMessage("PecaId deve ser maior que zero.");
            peca.RuleFor(x => x.Quantidade)
                .GreaterThan(0).WithMessage("Quantidade da peca deve ser maior que zero.");
        });
    }
}

