namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public sealed class InboxMessage
{
    private InboxMessage() { }

    public Guid MessageId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string Tipo { get; private set; } = null!;
    public DateTimeOffset ProcessadoEm { get; private set; }

    public static InboxMessage Criar(
        Guid messageId,
        Guid correlationId,
        string tipo,
        DateTimeOffset processadoEm) => new()
        {
            MessageId = messageId,
            CorrelationId = correlationId,
            Tipo = tipo,
            ProcessadoEm = processadoEm
        };
}
