using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("[controller]")]
public class MissionsController : ControllerBase
{
    private MissionService _missionServices;

    public MissionsController(IMongoDatabase database)
    {
        _missionServices = new MissionService(database);
    }

    // GET /missions
    [HttpGet]
    public async Task<ActionResult<List<Mission>>> Get()
    {
        var missions = await _missionServices.GetMissions();
        return Ok(missions);
    }

    // GET /missions/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Mission>> GetById(Guid id)
    {
        var missions = await _missionServices.GetMission(id);

        if (missions == null)
            return NotFound();

        return Ok(missions);
    }

    // GET /guilds/{id}/missions
    [HttpGet("/guilds/{guildId:guid}/missions")]
    public async Task<ActionResult<List<Mission>>> GetByGuild(Guid guildId)
    {
        var missions = await _missionServices.GetMissionsByGuild(guildId);

        return Ok(missions);
    }

    // POST /missions
    [HttpPost]
    public async Task<ActionResult<Mission>> Create(CreateMissionRequest request)
    {
        var mission = await _missionServices.CreateMissions(request);

        return CreatedAtAction(nameof(GetById), new { id = mission.Id }, mission);
    }

    // PUT /missions/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMissionRequest request)
    {
        if (!await _missionServices.UpdateMissions(id, request))
            return NotFound();

        return NoContent();
    }

    // DELETE /missions/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _missionServices.DeleteMissions(id))
            return NotFound();

        return NoContent();
    }

    // PATCH /missions/{id}
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, PatchMissionRequest request)
    {
        var result = await _missionServices.UpdateMission(id, request);

        return result switch
        {
            UpdateMissionResult.NotFound => NotFound(),
            UpdateMissionResult.NoFields => BadRequest("No fields to update."),
            UpdateMissionResult.Updated => NoContent(),
            _ => StatusCode(500)
        };
    }
}