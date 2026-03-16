using MongoDB.Driver;

public class AdventurousService
{
    private readonly IMongoCollection<Adventurous> _adventurous;

    public AdventurousService(IMongoDatabase database)
    {
        _adventurous = database.GetCollection<Adventurous>("adventurous");
    }

    public async Task<List<Adventurous>> GetAdventurous()
    {
        return await _adventurous.Find(_ => true).ToListAsync();
    }

    public async Task<Adventurous> GetAdventurous(Guid id)
    {
        return await _adventurous.Find(g => g.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Adventurous> CreateAdventurous(CreateAdventurousRequest request)
    {
        var Adventurous = new Adventurous
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Class = request.Class,
            Level = request.Level,
            Money = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _adventurous.InsertOneAsync(Adventurous);

        return Adventurous;
    }

    public async Task<bool> DeleteAdventurous(Guid id)
    {
        var result = await _adventurous.DeleteOneAsync(a => a.Id == id);

        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateAdventurous(Guid id, UpdateAdventurousRequest request)
    {
        var update = Builders<Adventurous>.Update
            .Set(a => a.Name, request.Name)
            .Set(a => a.Level, request.Level)
            .Set(a => a.Class, request.Class);

        var result = await _adventurous.UpdateOneAsync(a => a.Id == id, update);

        return result.MatchedCount > 0;
    }

    public async Task<UpdateAdventurousResult> UpdateAdventurous(Guid id, PatchAdventurousRequest request)
    {
        var updates = new List<UpdateDefinition<Adventurous>>();

        if (request.Name != null)
            updates.Add(Builders<Adventurous>.Update.Set(m => m.Name, request.Name));

        if (request.Class != null)
            updates.Add(Builders<Adventurous>.Update.Set(m => m.Class, request.Class));

        if (request.Level.HasValue)
            updates.Add(Builders<Adventurous>.Update.Set(m => m.Level, request.Level.Value));

        if (updates.Count == 0)
            return UpdateAdventurousResult.NoFields;

        var updateDefinition = Builders<Adventurous>.Update.Combine(updates);

        var result = await _adventurous.UpdateOneAsync(
            a => a.Id == id,
            updateDefinition
        );

        if (result.MatchedCount == 0)
            return UpdateAdventurousResult.NotFound;

        return UpdateAdventurousResult.Updated;
    }

    

}