using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
public class Guild
{
    [BsonId]
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DbId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name {get; set;}
    public Mission[] Missions { get; set; } = Array.Empty<Mission>();
    public DateTime CreatedAt {get; set;}

}