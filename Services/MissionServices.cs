using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

public class MissionService
{
    private readonly IMongoCollection<Mission> _missions;

    public MissionService(IMongoDatabase database)
    {
        _missions = database.GetCollection<Mission>("missions");
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

    public async Task<Mission> CreateMissions(CreateMissionRequest request)
    {
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

        return mission;
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

    public async Task<UpdateMissionResult> UpdateMission(Guid id, PatchMissionRequest request)
    {
        var updates = new List<UpdateDefinition<Mission>>();

        if (request.Name != null)
            updates.Add(Builders<Mission>.Update.Set(m => m.Name, request.Name));

        if (request.Task != null)
            updates.Add(Builders<Mission>.Update.Set(m => m.Task, request.Task));

        if (request.Reward.HasValue)
            updates.Add(Builders<Mission>.Update.Set(m => m.Reward, request.Reward.Value));

        if (request.Status.HasValue)
            updates.Add(Builders<Mission>.Update.Set(m => m.Status, request.Status.Value));

        if (updates.Count == 0)
            return UpdateMissionResult.NoFields;

        var updateDefinition = Builders<Mission>.Update.Combine(updates);

        var result = await _missions.UpdateOneAsync(
            m => m.Id == id,
            updateDefinition
        );

        if (result.MatchedCount == 0)
            return UpdateMissionResult.NotFound;

        return UpdateMissionResult.Updated;
    }

    public async Task<bool> DeleteMissions(Guid id)
    {
        var result = await _missions.DeleteOneAsync(g => g.Id == id);

        return result.DeletedCount > 0;
    }
}