using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System.Text.Json.Serialization;
public enum Status
{
    Available,
    Completed
}

public class Mission
{
    [BsonId]
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DbId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Task { get; set; }
    public float Reward { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
}