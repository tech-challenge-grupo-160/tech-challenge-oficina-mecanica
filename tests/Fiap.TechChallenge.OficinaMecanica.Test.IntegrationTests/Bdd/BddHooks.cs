using Reqnroll;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Bdd;

[Binding]
public sealed class BddHooks
{
    public static BddWebApplicationFactory Factory { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task IniciarApi()
    {
        Factory = new BddWebApplicationFactory();
        await ((IAsyncLifetime)Factory).InitializeAsync();
    }

    [AfterTestRun]
    public static async Task EncerrarApi()
    {
        if (Factory is not null)
            await ((IAsyncLifetime)Factory).DisposeAsync();
    }

    [BeforeScenario]
    public static void LimparBanco() => Factory.ResetDatabase();
}
