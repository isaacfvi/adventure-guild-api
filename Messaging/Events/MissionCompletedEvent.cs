public class MissionCompletedEvent : DomainEvent
{
    public override string EventType => "mission.completed";
    public Guid MissionId { get; init; }
    public Guid WinnerId { get; init; }
    public string MissionName { get; init; } = string.Empty;
    public float Reward { get; init; }
}
