using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("[controller]")]
public class AdventurousController : ControllerBase
{
    private AdventurousService _adventurousServices;
    private MissionAcceptedService _missionAcceptedServices;

    public AdventurousController(IMongoDatabase database)
    {
        _adventurousServices = new AdventurousService(database);
        _missionAcceptedServices = new MissionAcceptedService(database);
    }

    // GET /Adventurous
    [HttpGet]
    public async Task<ActionResult<List<Adventurous>>> Get()
    {
        var Adventurouss = await _adventurousServices.GetAdventurous();
        return Ok(Adventurouss);
    }

    // GET /Adventurous/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Adventurous>> GetById(Guid id)
    {
        var Adventurous = await _adventurousServices.GetAdventurous(id);

        if (Adventurous == null)
            return NotFound();

        return Ok(Adventurous);
    }

    [HttpGet("{id:guid}/missions")]
    public async Task<ActionResult<List<Mission>>> GetAdventurousMissions(Guid id)
    {
        var Missions = await _missionAcceptedServices.GetAdventurousMissions(id);

        if(Missions == null)
            return NotFound();

        return Ok(Missions);
    }

    // POST /Adventurous
    [HttpPost]
    public async Task<ActionResult<Adventurous>> Create(CreateAdventurousRequest request)
    {
        var Adventurous = await _adventurousServices.CreateAdventurous(request);

        return CreatedAtAction(nameof(GetById), new { id = Adventurous.Id }, Adventurous);
    }

    // PUT /Adventurous/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAdventurousRequest request)
    {
        if (!await _adventurousServices.UpdateAdventurous(id, request))
            return NotFound();

        return NoContent();
    }

    // DELETE /Adventurouss/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _adventurousServices.DeleteAdventurous(id))
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, PatchAdventurousRequest request)
    {
        var result = await _adventurousServices.UpdateAdventurous(id, request);

        return result switch
        {
            RestResult.NotFound => NotFound(),
            RestResult.NoFields => BadRequest("No fields to update."),
            RestResult.Updated => NoContent(),
            _ => StatusCode(500)
        };
    }
}