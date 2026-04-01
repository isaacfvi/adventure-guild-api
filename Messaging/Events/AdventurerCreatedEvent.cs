public class AdventurerCreatedEvent : DomainEvent
{
    public override string EventType => "adventurer.created";
    public Guid AdventurerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Class { get; init; } = string.Empty;
    public int Level { get; init; }
}