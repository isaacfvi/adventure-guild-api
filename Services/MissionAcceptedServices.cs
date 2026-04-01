using MongoDB.Driver;

public class MissionAcceptedService
{
    private readonly IMongoCollection<Mission> _missions;
    private readonly IMongoCollection<AcceptedMission> _acceptedMissions;
    private readonly IMongoCollection<Adventurous> _adventurous;
    private readonly IEventBus _eventBus;
    private readonly ILogger<MissionAcceptedService> _logger;

    public MissionAcceptedService(IMongoDatabase database, IEventBus eventBus, ILogger<MissionAcceptedService> logger)
    {
        _missions = database.GetCollection<Mission>("missions");
        _acceptedMissions = database.GetCollection<AcceptedMission>("acceptedMissions");
        _adventurous = database.GetCollection<Adventurous>("adventurous");
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<RestResult> AcceptMission(Guid id, AcceptMissionRequest request)
    {
        var mission = await _missions.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (mission == null)
            return RestResult.NotFound;

        if (mission.Status != MissionStatus.Available)
            return RestResult.Conflict;

        var alreadyAccepted = await _acceptedMissions
            .Find(x => x.MissionId == id && x.AdventurousId == request.AdventurousId)
            .AnyAsync();

        if (alreadyAccepted)
            return RestResult.Conflict;

        await CreateMission(id, request.AdventurousId);

        try
        {
            _eventBus.Publish(new MissionAcceptedEvent
            {
                MissionId = id,
                AdventurerId = request.AdventurousId,
                MissionName = mission.Name,
                Reward = mission.Reward
            }, "mission.accepted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar MissionAcceptedEvent para a missão {MissionId}", id);
        }

        return RestResult.NoContent;
    }

    private async Task CreateMission(Guid MissionId, Guid AdventurousId)
    {
        var mission = new AcceptedMission
        {
            Id = Guid.NewGuid(),
            MissionId = MissionId,
            AdventurousId = AdventurousId,
            Status = MissionAcceptedStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        await _acceptedMissions.InsertOneAsync(mission);
    }

    public async Task<List<Mission>> GetAdventurousMissions(Guid adventurousId)
    {
        var accepted = await _acceptedMissions
            .Find(x => x.AdventurousId == adventurousId)
            .ToListAsync();

        var missionIds = accepted.Select(x => x.MissionId).ToList();

        var missions = await _missions
            .Find(x => missionIds.Contains(x.Id))
            .ToListAsync();

        return missions;
    }

    public async Task<RestResult> CompleteMission(Guid id, CompleteMissionRequest request)
    {
        var acceptedMission = await GetAcceptedMission(id, request.AdventurousId);
        if (acceptedMission == null) return RestResult.NotFound;
        if (acceptedMission.Status == MissionAcceptedStatus.Completed) return RestResult.Conflict;

        var mission = await GetMission(id);

        await MarkOtherMissionsLost(id, acceptedMission.Id);
        await MarkMissionCompleted(acceptedMission.Id);
        await GiveReward(acceptedMission.AdventurousId, mission.Reward);
        await SetMissionWinner(id, acceptedMission.AdventurousId);

        try
        {
            _eventBus.Publish(new MissionCompletedEvent
            {
                MissionId = id,
                WinnerId = request.AdventurousId,
                MissionName = mission.Name,
                Reward = mission.Reward
            }, "mission.completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar MissionCompletedEvent para a missão {MissionId}", id);
        }

        return RestResult.NoContent;
    }

    private Task<AcceptedMission> GetAcceptedMission(Guid missionId, Guid adventurousId)
    {
        return _acceptedMissions
            .Find(x => x.MissionId == missionId && x.AdventurousId == adventurousId)
            .FirstOrDefaultAsync();
    }

    private Task<Mission> GetMission(Guid missionId)
    {
        return _missions
            .Find(x => x.Id == missionId)
            .FirstOrDefaultAsync();
    }

    private Task MarkOtherMissionsLost(Guid missionId, Guid winnerAcceptedId)
    {
        var update = Builders<AcceptedMission>.Update
            .Set(m => m.Status, MissionAcceptedStatus.Lost);
        return _acceptedMissions.UpdateManyAsync(
            m => m.MissionId == missionId && m.Id != winnerAcceptedId,
            update
        );
    }

    private Task MarkMissionCompleted(Guid acceptedId)
    {
        var update = Builders<AcceptedMission>.Update
            .Set(m => m.Status, MissionAcceptedStatus.Completed);
        return _acceptedMissions.UpdateOneAsync(
            m => m.Id == acceptedId,
            update
        );
    }

    private Task GiveReward(Guid adventurousId, float reward)
    {
        var update = Builders<Adventurous>.Update.Inc(a => a.Money, reward);
        return _adventurous.UpdateOneAsync(
            a => a.Id == adventurousId,
            update
        );
    }

    private Task SetMissionWinner(Guid missionId, Guid adventurousId)
    {
        var update = Builders<Mission>.Update.Set(m => m.WinnerAdventurous, adventurousId);
        return _missions.UpdateOneAsync(
            m => m.Id == missionId,
            update
        );
    }

}