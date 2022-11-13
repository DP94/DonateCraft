using System.Net.Http.Headers;
using System.Text.Json;
using Amazon.Lambda.Core;
using Common.Exceptions;
using Common.Models;
using Core.Services;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
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
    private readonly ICharityService _charityService;
    private readonly string _apiKey;
    private readonly string _donateCraftUi;

    private const int DONATION_ID = 0;
    private const int PLAYER_ID = 1;
    private const int DONOR_ID = 2;

    public CallbackController(HttpClient client, IDonationService donationService, ILockService lockService, IOptions<DonateCraftOptions> options, ICharityService charityService)
    {
        this._client = client;
        this._donationService = donationService;
        this._lockService = lockService;
        this._charityService = charityService;
        this._apiKey = options.Value.JustGivingApiKey;
        this._donateCraftUi = options.Value.DonateCraftUiUrl;
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string? data)
    {
        if (data == null)
        {
            LambdaLogger.Log($"No data received from callback initiator {data}, error code 1");
            return Redirect($"{this._donateCraftUi}?status=error&code=1");
        }
        var justGivingData = data.Split("~");
        var donationId = justGivingData[DONATION_ID];
        if (justGivingData.Length < 2)
        {
            LambdaLogger.Log($"Data received is malformed {data}, error code 2");
            return Redirect($"{this._donateCraftUi}?status=error&code=2");
        }
        var player = justGivingData[PLAYER_ID];
        var paidForKey = justGivingData.Length > 2 ? justGivingData[DONOR_ID] : null;
        if (string.IsNullOrWhiteSpace(donationId) || string.IsNullOrWhiteSpace(player))
        {
            LambdaLogger.Log($"Donation id or player id are missing {data}, error code 3");
            return Redirect($"{this._donateCraftUi}?status=error&code=3");
        }

        Lock? currentLock = null;
        try
        {
            currentLock = await this._lockService.GetById(player);
        }
        catch (ResourceNotFoundException)
        {
            LambdaLogger.Log($"Lock with id {player} not found, error code 4");
        }
        if (currentLock == null)
        {
            //In the event of someone donating when no lock is present
            return Redirect($"{this._donateCraftUi}?status=error&code=4");
        }
        if (currentLock.Unlocked)
        {
            //Send message here saying lock already unlocked
            return Redirect($"{this._donateCraftUi}?status=warning");
        }

        this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var justGivingDonation = await this.GetDonationData(donationId);
        if (justGivingDonation is not { Status: "Accepted" or "Pending" })
        {
            //Send error back here
            LambdaLogger.Log($"Donation was not successful! Status is {justGivingDonation?.Status}, error code 5");
            return Redirect($"{this._donateCraftUi}?status=error&code=5");
        }

        var charityData = await GetCharityData(justGivingDonation.CharityId);
        await this._donationService.Create(player, new Donation
        {
            Amount = Convert.ToDouble(justGivingDonation.Amount),
            Id = justGivingDonation.Id.ToString(),
            CharityId = justGivingDonation.CharityId,
            CharityName = charityData.Name,
            CreatedDate = DateTime.Now,
            PaidForId = paidForKey ?? player,
            Private = string.IsNullOrWhiteSpace(justGivingDonation.Amount)
        });
        var charity = await this._charityService.GetById(justGivingDonation.CharityId.ToString());
        charity.DonationCount++;
        await this._charityService.Update(charity);

        currentLock.DonationId = justGivingDonation.Id.ToString();
        currentLock.Unlocked = true;
        await this._lockService.Update(currentLock);
        return Redirect($"{this._donateCraftUi}/players?status=success");
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