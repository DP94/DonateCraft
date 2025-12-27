using Common.Models;
using Common.Models.Sort;
using Core.Services.Death;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/Player/{playerId}/[controller]")]
[EnableCors]
public class DeathController : WithPlayerIdController<Death>
{

    private readonly IDeathService _deathService;

    public DeathController(IDeathService deathService)
    {
        this._deathService = deathService;
    }

    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerOperation("Gets a death by id")]
    public async override Task<IActionResult> GetByIdForPlayer(string playerId, string id)
    {
        return Ok(await this._deathService.GetByPlayerId(playerId, id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerOperation("Gets all deaths")]
    public async override Task<IActionResult> GetAllForPlayer(string playerId)
    {
        return Ok(await this._deathService.GetAllForPlayerId(playerId));
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Death))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Creates a deaths")]
    public async override Task<IActionResult> Create(string playerId, [FromBody] Death death)
    {
        death.Id = Guid.NewGuid().ToString();
        death.CreatedDate = DateTime.UtcNow;
        death.PlayerId = playerId;
        var createdDeath = await this._deathService.Create(playerId, death);
        return Created($"{this.HttpContext?.Request.GetEncodedUrl()}/{death.Id}", createdDeath);
    }
    
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Updates a death")]
    public async override Task<IActionResult> Update(string playerId, [FromBody] Death death)
    {
        var updatedDeath = await this._deathService.Update(playerId, death);
        return Ok(updatedDeath);
    }
    
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Death))]
    [SwaggerOperation("Deletes a death")]
    public async override Task<IActionResult> Delete(string playerId, string id)
    {
        await this._deathService.Delete(playerId, id);
        return NoContent();
    }

    public override SortCriteriaBase<Death> CreateSortCriteria()
    {
        throw new NotImplementedException();
    }
}