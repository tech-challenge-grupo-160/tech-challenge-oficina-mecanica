namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface IBusinessMetrics
{
    void RecordOrderCreated();

    void RecordOrderCreationFailure(string reason);
}
