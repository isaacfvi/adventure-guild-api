using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

public class GuildService
{
    private readonly IMongoCollection<Guild> _guilds;

    public GuildService(IMongoDatabase database)
    {
        _guilds = database.GetCollection<Guild>("guilds");
    }

    public async Task<List<Guild>> GetGuilds()
    {
        return await _guilds
            .Find(_ => true)
            .ToListAsync();
    }

    public async Task<Guild> GetGuild(Guid id)
    {
        return await _guilds
        .Find(g => g.Id == id)
        .FirstOrDefaultAsync();
    }

    public async Task<Guild> CreateGuild(CreateGuildRequest request)
    {
        Guild guild = new Guild
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _guilds.InsertOneAsync(guild);

        return guild;
    }

    public async Task<bool> UpdateGuild(Guid id, UpdateGuildRequest request)
    {
        var update = Builders<Guild>.Update
            .Set(g => g.Name, request.Name);

        var result = await _guilds.UpdateOneAsync(g => g.Id == id, update);

        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteGuild(Guid id)
    {
        var result = await _guilds.DeleteOneAsync(g => g.Id == id);

        return result.DeletedCount > 0;
    }
}