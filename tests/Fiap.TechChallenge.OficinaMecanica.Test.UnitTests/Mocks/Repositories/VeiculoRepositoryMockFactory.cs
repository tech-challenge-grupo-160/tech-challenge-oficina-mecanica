using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Repositories;

public static class VeiculoRepositoryMockFactory
{
    public static Mock<IVeiculoRepository> CreateStrict() => new(MockBehavior.Strict);
}
