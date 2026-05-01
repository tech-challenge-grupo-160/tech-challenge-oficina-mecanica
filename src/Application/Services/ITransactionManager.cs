namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface ITransactionManager
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken);
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);
}
