using Common.Models;
using Common.Models.Sort;
using Core.Services.Donation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[EnableCors]
[Route("v1/Player/{playerId}/[controller]")]
public class DonationController : WithPlayerIdController<Donation>
{
    private readonly IDonationService _donationService;

    public DonationController(IDonationService donationService)
    {
        this._donationService = donationService;
    }
    
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Donation))]
    [SwaggerOperation("Gets a donation by id")]
    public async override Task<IActionResult> GetByIdForPlayer(string playerId, string id)
    {
        return Ok(await this._donationService.GetByPlayerId(playerId, id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Donation))]
    [SwaggerOperation("Gets all donations")]
    public async override Task<IActionResult> GetAllForPlayer(string playerId)
    {
        return Ok(await this._donationService.GetAllForPlayerId(playerId));
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Donation))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Creates a donations")]
    public async override Task<IActionResult> Create(string playerId, [FromBody] Donation donation)
    {
        donation.Id = Guid.NewGuid().ToString();
        donation.CreatedDate = DateTime.UtcNow;
        var createdDonation = await this._donationService.Create(playerId, donation);
        return Created($"{this.HttpContext?.Request.GetEncodedUrl()}/{donation.Id}", createdDonation);
    }
    
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Success", typeof(Donation))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Updates a donation")]
    public async override Task<IActionResult> Update(string playerId, [FromBody] Donation donation)
    {
        var updatedDonation = await this._donationService.Update(playerId, donation);
        return Ok(updatedDonation);
    }
    
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Donation))]
    [SwaggerOperation("Deletes a donation")]
    public async override Task<IActionResult> Delete(string playerId, string id)
    {
        await this._donationService.Delete(playerId, id);
        return NoContent();
    }

    public override SortCriteriaBase<Donation> CreateSortCriteria()
    {
        throw new NotImplementedException();
    }
}