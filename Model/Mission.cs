using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System.Text.Json.Serialization;

public class Mission
{
    [BsonId]
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DbId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Task { get; set; }
    public float Reward { get; set; }
    public Guid GuildId {get; set;}
    [BsonRepresentation(BsonType.String)]
    public Guid? WinnerAdventurous {get; set;}
    public MissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}