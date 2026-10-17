namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface IBusinessMetrics
{
    void RecordOrderCreated();

    void RecordOrderCreationFailure(string reason);

    void RecordOrderStageDuration(string stage, double durationSeconds);
}
