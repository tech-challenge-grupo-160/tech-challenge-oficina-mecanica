using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Repositories;

public static class VeiculoRepositoryMockFactory
{
    public static Mock<IVeiculoRepository> CreateStrict() => new(MockBehavior.Strict);
}
