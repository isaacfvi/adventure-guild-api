using MongoDB.Driver;

public class MissionService
{
    private readonly IMongoCollection<Mission> _missions;
    private readonly IMongoCollection<Guild> _guilds;

    public MissionService(IMongoDatabase database)
    {
        _missions = database.GetCollection<Mission>("missions");
        _guilds = database.GetCollection<Guild>("guilds");
    }

    public async Task<List<Mission>> GetMissions()
    {
        return await _missions.Find(_ => true).ToListAsync();
    }

    public async Task<Mission> GetMission(Guid id)
    {
        return await _missions.Find(g => g.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Mission>> GetMissionsByGuild(Guid guildId)
    {
        return await _missions
            .Find(m => m.GuildId == guildId)
            .ToListAsync();
    }

    public async Task<(RestResult, Mission?)> CreateMissions(CreateMissionRequest request)
    {
        var guildExists = await _guilds.Find(g => g.Id == request.GuildId).AnyAsync();
        if (!guildExists)
            return (RestResult.NotFound, null);

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Task = request.Task,
            Reward = request.Reward,
            GuildId = request.GuildId,
            Status = MissionStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        await _missions.InsertOneAsync(mission);

        return (RestResult.Ok, mission);
    }

    public async Task<bool> UpdateMissions(Guid id, UpdateMissionRequest request)
    {
        var update = Builders<Mission>.Update
            .Set(m => m.Name, request.Name)
            .Set(m => m.Task, request.Task)
            .Set(m => m.Reward, request.Reward);

        var result = await _missions.UpdateOneAsync(m => m.Id == id, update);

        return result.MatchedCount > 0;
    }

    public async Task<RestResult> UpdateMission(Guid id, PatchMissionRequest request)
    {
        var updates = new List<UpdateDefinition<Mission>>();

        if (request.Name != null)
            updates.Add(Builders<Mission>.Update.Set(m => m.Name, request.Name));

        if (request.Task != null)
            updates.Add(Builders<Mission>.Update.Set(m => m.Task, request.Task));

        if (request.Reward.HasValue)
            updates.Add(Builders<Mission>.Update.Set(m => m.Reward, request.Reward.Value));

        if (request.Status.HasValue)
        {
            var current = await _missions.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (current == null)
                return RestResult.NotFound;

            if (current.Status == MissionStatus.Completed && request.Status.Value == MissionStatus.Available)
                return RestResult.BadRequest;

            updates.Add(Builders<Mission>.Update.Set(m => m.Status, request.Status.Value));
        }

        if (updates.Count == 0)
            return RestResult.NoFields;

        var updateDefinition = Builders<Mission>.Update.Combine(updates);

        var result = await _missions.UpdateOneAsync(
            m => m.Id == id,
            updateDefinition
        );

        if (result.MatchedCount == 0)
            return RestResult.NotFound;

        return RestResult.Updated;
    }

    public async Task<bool> DeleteMissions(Guid id)
    {
        var result = await _missions.DeleteOneAsync(g => g.Id == id);

        return result.DeletedCount > 0;
    }
}