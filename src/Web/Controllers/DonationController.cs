using Common.Models;
using Core.Services;
using Core.Services.Donation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[EnableCors]
[Route("v1/Player/{playerId}/[controller]")]
public class DonationController : ControllerBase
{
    private readonly IDonationService _donationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DonationController(IDonationService donationService, IHttpContextAccessor httpContextAccessor)
    {
        this._donationService = donationService;
        this._httpContextAccessor = httpContextAccessor;
    }
    
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Donation))]
    [SwaggerOperation("Gets a donation by id")]
    public async Task<IActionResult> GetDonation(string playerId, string id)
    {
        return Ok(await this._donationService.GetDonation(playerId, id));
    }
    
    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Donation))]
    [SwaggerOperation("Gets all donations")]
    public async Task<IActionResult> GetDonations(string playerId)
    {
        return Ok(await this._donationService.GetDonations(playerId));
    }
    
    [HttpPost]
    [SwaggerResponse(201, "Success", typeof(Donation))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Creates a donations")]
    public async Task<IActionResult> CreateDonation(string playerId, [FromBody] Donation donation)
    {
        donation.Id = Guid.NewGuid().ToString();
        donation.CreatedDate = DateTime.UtcNow;
        var createdDonation = await this._donationService.Create(playerId, donation);
        return Created($"{this._httpContextAccessor.HttpContext?.Request.GetEncodedUrl()}/{donation.Id}", createdDonation);
    }
    
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Success", typeof(Donation))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerOperation("Updates a donation")]
    public async Task<IActionResult> UpdateDonation(string playerId, [FromBody] Donation donation)
    {
        var updatedDonation = await this._donationService.UpdateDonation(playerId, donation);
        return Ok(updatedDonation);
    }
    
    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Success", typeof(Donation))]
    [SwaggerOperation("Deletes a donation")]
    public async Task<IActionResult> DeleteDonation(string playerId, string id)
    {
        await this._donationService.DeleteDonation(playerId, id);
        return NoContent();
    }
}