using Common.Models;
using Core.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class DeathController : ControllerBase
{

    private IDeathService _deathService;
    private HttpContextAccessor _httpContextAccessor;

    public DeathController(IDeathService deathService, HttpContextAccessor httpContextAccessor)
    {
        this._deathService = deathService;
        this._httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetDeath(string id)
    {
        return Ok(await this._deathService.GetDeathById(id));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetDeaths()
    {
        return Ok(await this._deathService.GetDeaths());
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateDeath(Death death)
    {
        if (string.IsNullOrWhiteSpace(death.PlayerId))
        {
            return BadRequest("A death must have a player ID associated with it");
        }
        
        var createdDeath = await this._deathService.CreateDeath(death);
        return Created($"{this._httpContextAccessor.HttpContext?.Request.GetEncodedUrl()}/{death.Id}", createdDeath);
    }
    
    [HttpPut]
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
    
    [HttpDelete]
    public async Task<IActionResult> DeleteDeath(string id)
    {
        await this._deathService.DeleteDeath(id);
        return NoContent();
    }
}