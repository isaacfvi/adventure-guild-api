using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System.Text.Json.Serialization;
class Shop
{
    [BsonId]
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DbId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public Item[] Items { get; set; } = Array.Empty<Item>();
    public required string Category {get; set;}
    public float Money {get; set;}
    public DateTime CreatedAt { get; set; }
}