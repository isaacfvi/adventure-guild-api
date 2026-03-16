using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System.Text.Json.Serialization;
public class Adventurous
{
    [BsonId]
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DbId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required int Level {get; set;}
    public required string Class {get; set;}
    public float Money { get; set; }
    public DateTime CreatedAt { get; set; }
}