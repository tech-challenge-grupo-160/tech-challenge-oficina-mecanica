using Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.PedidosCompra;

public sealed class CriarPedidoCompraCommandValidator : AbstractValidator<CriarPedidoCompraCommand>
{
    public CriarPedidoCompraCommandValidator()
    {
        RuleFor(x => x.OrdemDeServicoId)
            .GreaterThan(0).WithMessage("Ordem de servico do pedido de compra e obrigatoria.");

        RuleFor(x => x.PecaId)
            .GreaterThan(0).WithMessage("Peca do pedido de compra e obrigatoria.");

        RuleFor(x => x.QuantidadeSolicitada)
            .GreaterThan(0).WithMessage("A quantidade solicitada deve ser maior que zero.");

        RuleFor(x => x.Observacao)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacao));
    }
}
