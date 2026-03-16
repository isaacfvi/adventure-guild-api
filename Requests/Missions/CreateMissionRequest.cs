public class CreateMissionRequest
{
    public string Name { get; set; }
    public string Task { get; set; }
    public float Reward { get; set; }
    public Guid GuildId {get; set;}
}