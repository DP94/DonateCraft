using Common.Models;
using Core.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class CharityController : ControllerBase
{
    private readonly ICharityService _charityService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CharityController(ICharityService charityService, IHttpContextAccessor httpContextAccessor)
    {
        this._charityService = charityService;
        this._httpContextAccessor = httpContextAccessor;
    }
    
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Charity))]
    [SwaggerOperation("Gets a charity by id")]
    public async Task<IActionResult> GetCharity(string id)
    {
        return Ok(await this._charityService.GetCharityById(id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Charity))]
    [SwaggerOperation("Gets all charities")]
    public async Task<IActionResult> GetCharities()
    {
        return Ok(await this._charityService.GetCharities());
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Charity))]
    [SwaggerOperation("Creates a charity")]
    public async Task<IActionResult> CreateCharity([FromBody] Charity charity)
    {
        var newCharity = await this._charityService.CreateCharity(charity);
        return Created($"{this._httpContextAccessor.HttpContext?.Request.GetEncodedUrl()}/{newCharity.Id}", newCharity);
    }
    
    [HttpPut]
    [SwaggerResponse(200, "Success", typeof(Charity))]
    [SwaggerOperation("Updates a charity")]
    public async Task<IActionResult> UpdateCharity([FromBody] Charity charity)
    {
        var updateCharity = await this._charityService.UpdateCharity(charity);
        return Ok(updateCharity);
    }
    
        
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Charity))]
    [SwaggerOperation("Deletes a charity")]
    public async Task<IActionResult> DeleteCharity(string id)
    {
        await this._charityService.DeleteCharity(id);
        return NoContent();
    }
}