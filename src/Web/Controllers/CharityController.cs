using Common.Models;
using Common.Models.Sort;
using Core.Services;
using Core.Services.Charity;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class CharityController : DonateCraftBaseController<Charity>
{
    private readonly ICharityService _charityService;

    public CharityController(ICharityService charityService)
    {
        this._charityService = charityService;
    }
    
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Charity))]
    [SwaggerOperation("Gets a charity by id")]
    public async override Task<IActionResult> GetById(string id)
    {
        return Ok(await this._charityService.GetById(id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Charity))]
    [SwaggerOperation("Gets all charities")]
    public async override Task<IActionResult> GetAll()
    {
        var charities = await this._charityService.GetAll();
        ProcessSorting(charities);
        return Ok(charities);
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Charity))]
    [SwaggerOperation("Creates a charity")]
    public async override Task<IActionResult> Create([FromBody] Charity charity)
    {
        var newCharity = await this._charityService.Create(charity);
        return Created($"{this.HttpContext?.Request.GetEncodedUrl()}/{newCharity.Id}", newCharity);
    }
    
    [HttpPut]
    [SwaggerResponse(200, "Success", typeof(Charity))]
    [SwaggerOperation("Updates a charity")]
    public async override Task<IActionResult> Update([FromBody] Charity charity)
    {
        var updateCharity = await this._charityService.Update(charity);
        return Ok(updateCharity);
    }
    
        
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Charity))]
    [SwaggerOperation("Deletes a charity")]
    public async override Task<IActionResult> Delete(string id)
    {
        await this._charityService.Delete(id);
        return NoContent();
    }

    public override SortCriteriaBase<Charity> CreateSortCriteria()
    {
        return new CharitySortCriteria();
    }
}