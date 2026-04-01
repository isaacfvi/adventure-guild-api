using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

[ApiController]
[Route("[controller]")]
public class MissionsController : ControllerBase
{
    private MissionService _missionServices;
    private MissionAcceptedService _acceptedMissionServices;

    public MissionsController(IMongoDatabase database, IEventBus eventBus, ILogger<MissionAcceptedService> logger)
    {
        _missionServices = new MissionService(database);
        _acceptedMissionServices = new MissionAcceptedService(database, eventBus, logger);
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

    // POST /missions/{id}/accept
    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<AcceptedMission>> Accept(Guid id, AcceptMissionRequest request)
    {
        var result = await _acceptedMissionServices.AcceptMission(id, request);

        return result switch
        {
            RestResult.NotFound => NotFound(),
            RestResult.Conflict => BadRequest("Mission is already completed"),
            RestResult.NoContent => NoContent(),
            _ => StatusCode(500)
        };
    }

    // POST /missions/{id}/complete
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<AcceptedMission>> Complete(Guid id, CompleteMissionRequest request)
    {
        var result = await _acceptedMissionServices.CompleteMission(id, request);

        return result switch
        {
            RestResult.NotFound => NotFound(),
            RestResult.Conflict => BadRequest("Mission is already completed"),
            RestResult.NoContent => NoContent(),
            _ => StatusCode(500)
        };
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
            RestResult.NotFound => NotFound(),
            RestResult.NoFields => BadRequest("No fields to update."),
            RestResult.Updated => NoContent(),
            _ => StatusCode(500)
        };
    }
}