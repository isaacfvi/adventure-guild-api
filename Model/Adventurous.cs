using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System.Text.Json.Serialization;
class adventurous
{
    [BsonId]
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DbId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public float Money { get; set; }
    public Mission? CurrentMission {get; set;}
    public DateTime CreatedAt { get; set; }
}