using Common.Models;
using Common.Models.Sort;
using Core.Services;
using Core.Services.Lock;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class LockController : WithIdController<Lock>
{
    private readonly ILockService _lockService;

    public LockController(ILockService lockService)
    {
        this._lockService = lockService;
    }
    
    
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Lock))]
    [SwaggerOperation("Gets a lock by id")]
    public async override Task<IActionResult> GetById(string id)
    {
        return Ok(await this._lockService.GetById(id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Lock))]
    [SwaggerOperation("Gets all locks")]
    public async override Task<IActionResult> GetAll()
    {
        var playerIds = this.HttpContext.Request.Query["playerIds"];
        var locks = new List<Lock>();
        if (playerIds is { Count: > 0 })
        {
            locks = await this._lockService.GetLocksForPlayers(playerIds.ToList());
        }
        else
        {
            locks = await this._lockService.GetAll();
        }
        ProcessSorting(locks);
        return Ok(locks);
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Lock))]
    [SwaggerOperation("Creates a lock")]
    public async override Task<IActionResult> Create([FromBody] [SwaggerRequestBody("The lock to create - sets the ID and Unlocked value automatically")] Lock aLock)
    {
        aLock.Id = Guid.NewGuid().ToString();
        aLock.Unlocked = false;
        var newLock = await this._lockService.Create(aLock);
        return Created($"{this.HttpContext?.Request.GetEncodedUrl()}/{aLock.Id}", newLock);
    }
    
    [HttpPut]
    [SwaggerResponse(200, "Success", typeof(Lock))]
    [SwaggerOperation("Updates a lock")]
    public async override Task<IActionResult> Update([FromBody] Lock aLock)
    {
        //Retrieve lock to see if it exists; throws exception if not
        await this.GetById(aLock.Id);
        var newLock = await this._lockService.Update(aLock);
        return Ok(newLock);
    }
    
        
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Lock))]
    [SwaggerOperation("Deletes a lock")]
    public async override Task<IActionResult> Delete(string id)
    {
        await this._lockService.Delete(id);
        return NoContent();
    }

    public override SortCriteriaBase<Lock> CreateSortCriteria()
    {
        return new LockSortCriteria();
    }
}