using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("[controller]")]
public class GuildsController : ControllerBase
{
    private GuildService _guildServices;

    public GuildsController(IMongoDatabase database)
    {
        _guildServices = new GuildService(database);
    }

    // GET /guilds
    [HttpGet]
    public async Task<ActionResult<List<Guild>>> Get()
    {
        var guilds = await _guildServices.GetGuilds();
        return Ok(guilds);
    }

    // GET /guilds/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Guild>> GetById(Guid id)
    {
        var guild = _guildServices.GetGuild(id);

        if (guild == null)
            return NotFound();

        return Ok(guild);
    }

    // POST /guilds
    [HttpPost]
    public async Task<ActionResult<Guild>> Create(CreateGuildRequest request)
    {
        var guild = await _guildServices.CreateGuild(request);

        return CreatedAtAction(nameof(GetById), new { id = guild.Id }, guild);
    }

    // PUT /guilds/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateGuildRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (!await _guildServices.UpdateGuild(id, request))
            return NotFound();

        return NoContent();
    }

    // DELETE /guilds/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _guildServices.DeleteGuild(id))
            return NotFound();

        return NoContent();
    }
}