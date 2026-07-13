using Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;
using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Validators.PedidosCompra;

public sealed class ListarPedidosCompraPorOrdemQueryValidator : AbstractValidator<ListarPedidosCompraPorOrdemQuery>
{
    public ListarPedidosCompraPorOrdemQueryValidator()
    {
        RuleFor(x => x.OrdemDeServicoId)
            .GreaterThan(0).WithMessage("Id da ordem de servico deve ser maior que zero.");
    }
}
