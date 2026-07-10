using System.Reflection;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Mocks.Entities;

public static class EntityMockHelper
{
    public static T WithId<T>(this T entity, int id)
        where T : class
    {
        SetPrivateProperty(entity, "Id", id);
        return entity;
    }

    public static T SetPrivateProperty<T>(this T entity, string propertyName, object? value)
        where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Property {propertyName} not found on {typeof(T).Name}.");

        property.SetValue(entity, value);
        return entity;
    }
}
