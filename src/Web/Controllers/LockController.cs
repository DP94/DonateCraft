using Common.Models;
using Core.Services;
using Core.Services.Lock;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class LockController : ControllerBase
{
    private readonly ILockService _lockService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LockController(ILockService lockService, IHttpContextAccessor httpContextAccessor)
    {
        this._lockService = lockService;
        this._httpContextAccessor = httpContextAccessor;
    }
    
    
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Lock))]
    [SwaggerOperation("Gets a lock by id")]
    public async Task<IActionResult> GetLock(string id)
    {
        return Ok(await this._lockService.GetLock(id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Lock))]
    [SwaggerOperation("Gets all locks")]
    public async Task<IActionResult> GetLocks([FromQuery] List<string> playerIds)
    {
        if (playerIds is { Count: > 0 })
        {
            return Ok(await this._lockService.GetLocksForPlayers(playerIds));
        }
        return Ok(await this._lockService.GetLocks());
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Lock))]
    [SwaggerOperation("Creates a lock")]
    public async Task<IActionResult> CreateLock([FromBody] [SwaggerRequestBody("The lock to create - sets the ID and Unlocked value automatically")] Lock aLock)
    {
        aLock.Id = Guid.NewGuid().ToString();
        aLock.Unlocked = false;
        var newLock = await this._lockService.Create(aLock);
        return Created($"{this._httpContextAccessor.HttpContext?.Request.GetEncodedUrl()}/{aLock.Id}", newLock);
    }
    
    [HttpPut]
    [SwaggerResponse(200, "Success", typeof(Lock))]
    [SwaggerOperation("Updates a lock")]
    public async Task<IActionResult> UpdateLock([FromBody] Lock aLock)
    {
        //Retrieve lock to see if it exists; throws exception if not
        await this.GetLock(aLock.Id);
        var newLock = await this._lockService.UpdateLock(aLock);
        return Ok(newLock);
    }
    
        
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Lock))]
    [SwaggerOperation("Deletes a lock")]
    public async Task<IActionResult> DeleteLock(string id)
    {
        await this._lockService.DeleteLock(id);
        return NoContent();
    }
}