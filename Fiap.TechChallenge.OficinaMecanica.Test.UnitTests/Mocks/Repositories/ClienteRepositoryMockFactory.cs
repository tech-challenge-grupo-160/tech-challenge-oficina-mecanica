using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Repositories;

public static class ClienteRepositoryMockFactory
{
    public static Mock<IClienteRepository> CreateStrict() => new(MockBehavior.Strict);
}
