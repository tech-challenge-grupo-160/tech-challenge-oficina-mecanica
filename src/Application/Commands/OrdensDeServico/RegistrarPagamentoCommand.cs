using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class RegistrarPagamentoCommand : IRequest<OrdemDeServicoDto>
{
    public int Id { get; init; }
}

