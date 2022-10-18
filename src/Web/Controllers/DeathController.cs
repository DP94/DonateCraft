using Common.Models;
using Core.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/[controller]")]
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
    public async Task<IActionResult> GetDeath(string id)
    {
        return Ok(await this._deathService.GetDeathById(id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerOperation("Gets all deaths")]
    public async Task<IActionResult> GetDeaths()
    {
        return Ok(await this._deathService.GetDeaths());
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Death))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Creates a deaths")]
    public async Task<IActionResult> CreateDeath([FromBody] Death death)
    {
        if (string.IsNullOrWhiteSpace(death.PlayerId))
        {
            return BadRequest("A death must have a player ID associated with it");
        }
        death.Id = Guid.NewGuid().ToString();
        death.CreatedDate = DateTime.UtcNow;
        var createdDeath = await this._deathService.CreateDeath(death);
        return Created($"{this._httpContextAccessor.HttpContext?.Request.GetEncodedUrl()}/{death.Id}", createdDeath);
    }
    
    [HttpPut]
    [SwaggerResponse(200, "Success", typeof(Death))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Updates a death")]
    public async Task<IActionResult> UpdateDeath(Death death)
    {
        var existingDeath = await this._deathService.GetDeathById(death.Id);
        if (existingDeath.PlayerId != death.PlayerId)
        {
            return BadRequest("Death player ID cannot be changed once created");
        }
        var updatedDeath = await this._deathService.UpdateDeath(death);
        return Ok(updatedDeath);
    }
    
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Death))]
    [SwaggerOperation("Deletes a death")]
    public async Task<IActionResult> DeleteDeath(string id)
    {
        await this._deathService.DeleteDeath(id);
        return NoContent();
    }
}