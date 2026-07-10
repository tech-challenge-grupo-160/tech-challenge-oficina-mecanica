using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Repositories;

public static class ClienteRepositoryMockFactory
{
    public static Mock<IClienteRepository> CreateStrict() => new(MockBehavior.Strict);
}
