using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("[controller]")]
public class GuildsController : ControllerBase
{
    private readonly IMongoCollection<Guild> _guilds;

    public GuildsController(IMongoDatabase database)
    {
        _guilds = database.GetCollection<Guild>("guilds");
    }

    // GET /guilds
    [HttpGet]
    public async Task<ActionResult<List<Guild>>> Get()
    {
        var guilds = await _guilds.Find(_ => true).ToListAsync();
        return Ok(guilds);
    }

    // GET /guilds/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Guild>> GetById(Guid id)
    {
        var guild = await _guilds.Find(g => g.Id == id).FirstOrDefaultAsync();

        if (guild == null)
            return NotFound();

        return Ok(guild);
    }

    // POST /guilds
    [HttpPost]
    public async Task<ActionResult<Guild>> Create(Guild guild)
    {
        guild.CreatedAt = DateTime.UtcNow;
        guild.Id = Guid.NewGuid();
        await _guilds.InsertOneAsync(guild);

        return CreatedAtAction(nameof(GetById), new { id = guild.Id }, guild);
    }

    // PUT /guilds/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Guild updatedGuild)
    {
        var update = Builders<Guild>.Update
            .Set(g => g.Name, updatedGuild.Name);

        var result = await _guilds.UpdateOneAsync(g => g.Id == id, update);

        if (result.MatchedCount == 0)
            return NotFound();

        return NoContent();
    }

    // DELETE /guilds/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _guilds.DeleteOneAsync(g => g.Id == id);

        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }
}