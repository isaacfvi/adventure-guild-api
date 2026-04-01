public class MissionAcceptedEvent : DomainEvent
{
    public override string EventType => "mission.accepted";
    public Guid MissionId { get; init; }
    public Guid AdventurerId { get; init; }
    public string MissionName { get; init; } = string.Empty;
    public float Reward { get; init; }
}
