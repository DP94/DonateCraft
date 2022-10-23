using Common.Models;
using Core.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/Player/{playerId}/[controller]")]
[EnableCors]
public class DeathController : ControllerBase
{

    private readonly IDeathService _deathService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeathController(IDeathService deathService, IHttpContextAccessor httpContextAccessor)
    {
        this._deathService = deathService;
        this._httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerOperation("Gets a death by id")]
    public async Task<IActionResult> GetDeath(string playerId, string id)
    {
        return Ok(await this._deathService.GetDeathById(playerId, id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerOperation("Gets all deaths")]
    public async Task<IActionResult> GetDeaths(string playerId)
    {
        return Ok(await this._deathService.GetDeaths(playerId));
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Death))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Creates a deaths")]
    public async Task<IActionResult> CreateDeath(string playerId, [FromBody] Death death)
    {
        death.Id = Guid.NewGuid().ToString();
        death.CreatedDate = DateTime.UtcNow;
        death.PlayerId = playerId;
        var createdDeath = await this._deathService.CreateDeath(playerId, death);
        return Created($"{this._httpContextAccessor.HttpContext?.Request.GetEncodedUrl()}/{death.Id}", createdDeath);
    }
    
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Updates a death")]
    public async Task<IActionResult> UpdateDeath(string playerId, [FromBody] Death death)
    {
        var updatedDeath = await this._deathService.UpdateDeath(playerId, death);
        return Ok(updatedDeath);
    }
    
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Death))]
    [SwaggerOperation("Deletes a death")]
    public async Task<IActionResult> DeleteDeath(string playerId, string id)
    {
        await this._deathService.DeleteDeath(playerId, id);
        return NoContent();
    }
}