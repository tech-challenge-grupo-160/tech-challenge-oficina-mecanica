using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks;

public static class ClienteRepositoryMockFactory
{
    public static Mock<IClienteRepository> CreateStrict() => new(MockBehavior.Strict);
}
