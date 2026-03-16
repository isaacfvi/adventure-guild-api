public class CreateMissionRequest
{
    public required string Name { get; set; }
    public required string Task { get; set; }
    public float Reward { get; set; }
    public Guid GuildId {get; set;}
}