using Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.PedidosCompra;

public sealed class ReceberPedidoCompraCommandValidator : AbstractValidator<ReceberPedidoCompraCommand>
{
    public ReceberPedidoCompraCommandValidator()
    {
        RuleFor(x => x.PedidoCompraId)
            .GreaterThan(0).WithMessage("Id do pedido de compra deve ser maior que zero.");

        RuleFor(x => x.QuantidadeRecebida)
            .GreaterThan(0).WithMessage("A quantidade recebida deve ser maior que zero.");
    }
}
