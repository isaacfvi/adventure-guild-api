using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("[controller]")]
public class MissionsController : ControllerBase
{
    private readonly IMongoCollection<Mission> _missions;

    public MissionsController(IMongoDatabase database)
    {
        _missions = database.GetCollection<Mission>("missions");
    }

    // GET /missions
    [HttpGet]
    public async Task<ActionResult<List<Mission>>> Get()
    {
        var missions = await _missions.Find(_ => true).ToListAsync();
        return Ok(missions);
    }

    // GET /missions/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Mission>> GetById(Guid id)
    {
        var missions = await _missions.Find(g => g.Id == id).FirstOrDefaultAsync();

        if (missions == null)
            return NotFound();

        return Ok(missions);
    }

    // GET /guilds/{id}/missions
    [HttpGet("/guilds/{guildId:guid}/missions")]
    public async Task<ActionResult<List<Mission>>> GetByGuild(Guid guildId)
    {
        var missions = await _missions
            .Find(m => m.GuildId == guildId)
            .ToListAsync();

        return Ok(missions);
    }

    // POST /missions
    [HttpPost]
    public async Task<ActionResult<Mission>> Create(CreateMissionRequest request)
    {
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Task = request.Task,
            Reward = request.Reward,
            GuildId = request.GuildId,
            Status = Status.Available,
            CreatedAt = DateTime.UtcNow
        };

        await _missions.InsertOneAsync(mission);

        return CreatedAtAction(nameof(GetById), new { id = mission.Id }, mission);
    }

    // PUT /missions/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMissionRequest request)
    {
        var update = Builders<Mission>.Update
            .Set(m => m.Name, request.Name)
            .Set(m => m.Task, request.Task)
            .Set(m => m.Reward, request.Reward);

        var result = await _missions.UpdateOneAsync(m => m.Id == id, update);

        if (result.MatchedCount == 0)
            return NotFound();

        return NoContent();
    }

    // DELETE /missions/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _missions.DeleteOneAsync(g => g.Id == id);

        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }

    // PATCH /missions/{id}
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, PatchMissionRequest request)
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
            return BadRequest("No fields to update.");

        var updateDefinition = Builders<Mission>.Update.Combine(updates);

        var result = await _missions.UpdateOneAsync(
            m => m.Id == id,
            updateDefinition
        );

        if (result.MatchedCount == 0)
            return NotFound();

        return NoContent();
    }
}