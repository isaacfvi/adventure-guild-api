using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
public class AcceptedMission
{
    [BsonId]
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DbId { get; set; }
    public Guid Id { get; set; }
    public Guid MissionId { get; set; }
    public Guid AdventurousId { get; set; }
    [BsonRepresentation(BsonType.String)]
    public MissionAcceptedStatus Status {get; set;}
    public DateTime CreatedAt {get; set;}
}

