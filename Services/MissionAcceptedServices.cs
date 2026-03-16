using MongoDB.Driver;

public class MissionAcceptedService
{
    private readonly IMongoCollection<Mission> _missions;
    private readonly IMongoCollection<AcceptedMission> _acceptedMissions;
    private readonly IMongoCollection<Adventurous> _adventurous;

    public MissionAcceptedService(IMongoDatabase database)
    {
        _missions = database.GetCollection<Mission>("missions");
        _acceptedMissions = database.GetCollection<AcceptedMission>("acceptedMissions");
        _adventurous = database.GetCollection<Adventurous>("acceptedMissions");
    }

    public async Task<RestResult> AcceptMission(Guid id, AcceptMissionRequest request)
    {   
        var mission = await _missions.Find(g => g.Id == id).FirstOrDefaultAsync();

        if (mission == null)
            return RestResult.NotFound;

        if(mission.Status != MissionStatus.Available) // Check se a missão já não foi concluída
            return RestResult.Conflict;

        CreateMission(id, request.AdventurousId);
        
        return RestResult.NoContent;
    }

    private void CreateMission(Guid Missionid, Guid AdventurousId)
    {
        var mission = new AcceptedMission
        {
            Id = Guid.NewGuid(),
            MissionId = Missionid,
            AdventurousId = AdventurousId,
            Status = MissionAcceptedStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        _acceptedMissions.InsertOneAsync(mission);
    }


}