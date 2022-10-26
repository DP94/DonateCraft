using System.Net.Http.Headers;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using Common.Models;
using Core.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class CallbackController : ControllerBase
{
    private readonly HttpClient _client;
    private readonly IDonationService _donationService;
    private readonly ILockService _lockService;
    private readonly string _apiKey;
    private readonly string _donateCraftUi;

    public const int DONATION_ID = 0;
    public const int PLAYER_ID = 1;
    public const int DONOR_ID = 2;

    public CallbackController(HttpClient client, IDonationService donationService, ILockService lockService, IOptions<DonateCraftOptions> options)
    {
        this._client = client;
        this._donationService = donationService;
        this._lockService = lockService;
        this._apiKey = options.Value.JustGivingApiKey;
        this._donateCraftUi = options.Value.DonateCraftUiUrl;
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string data)
    {
        var justGivingData = data.Split("|");
        var donationId = justGivingData[DONATION_ID];
        if (justGivingData.Length < 2)
        {
            return BadRequest("Invalid data returned from JustGiving!");
        }
        var player = justGivingData[PLAYER_ID];
        var paidForKey = justGivingData.Length > 2 ? justGivingData[DONOR_ID] : null;
        if (string.IsNullOrWhiteSpace(donationId) || string.IsNullOrWhiteSpace(player))
        {
            return BadRequest("Player or donation id is missing");
        }

        Lock currentLock = null;
        try
        {
            currentLock = await this._lockService.GetLock(player);
        }
        catch (ResourceNotFoundException)
        {
            Console.WriteLine($"Lock with id {player} not found");
        }
        if (currentLock == null)
        {
            //In the event of someone donating when no lock is present
            return Redirect(this._donateCraftUi);
        }
        if (currentLock.Unlocked)
        {
            //Send message here saying lock already unlocked
            return Redirect(this._donateCraftUi);
        }

        this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var justGivingDonation = await this.GetDonationData(donationId);
        if (justGivingDonation is not { Status: "Accepted" or "Pending" })
        {
            //Send error back here
            return Redirect(this._donateCraftUi);
        }

        var charityData = await GetCharityData(justGivingDonation.CharityId);
        await this._donationService.Create(player, new Donation
        {
            Amount = Convert.ToDouble(justGivingDonation.Amount),
            Id = justGivingDonation.Id.ToString(),
            CharityId = justGivingDonation.CharityId,
            CharityName = charityData.Name,
            CreatedDate = DateTime.Now,
            PaidForId = paidForKey ?? player
        });

        currentLock.DonationId = justGivingDonation.Id.ToString();
        currentLock.Unlocked = true;
        await this._lockService.UpdateLock(currentLock);
        return Redirect(this._donateCraftUi);
    }

    private async Task<JustGivingDonation> GetDonationData(string donationId)
    {
        var donationData = await this._client.GetAsync($"{this._apiKey}/v1/donation/{donationId}");
        donationData.EnsureSuccessStatusCode();
        var responseBody = await donationData.Content.ReadAsStringAsync();
        var justGivingDonation = JsonSerializer.Deserialize<JustGivingDonation>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (!donationData.IsSuccessStatusCode || justGivingDonation == null)
        {
            throw new BadHttpRequestException($"Could not find a donation with id of {donationId}");
        }
        return justGivingDonation;
    }

    private async Task<JustGivingCharity> GetCharityData(int charityId)
    {
        var charityResponse = await this._client.GetAsync($"{this._apiKey}/v1/charity/{charityId}");
        var charityData = JsonSerializer.Deserialize<JustGivingCharity>(await charityResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (!charityResponse.IsSuccessStatusCode || charityData == null)
        {
            throw new BadHttpRequestException($"Could not find a charity with id of {charityId}");
        }
        return charityData;
    }
}