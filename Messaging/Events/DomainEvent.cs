public abstract class DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public abstract string EventType { get; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
