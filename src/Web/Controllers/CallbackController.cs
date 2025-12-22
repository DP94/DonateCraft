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
    private readonly ILogger<CallbackController> _logger;

    private const int DONATION_ID = 0;
    private const int PLAYER_ID = 1;
    private const int DONOR_ID = 2;

    public CallbackController(HttpClient client, IDonationService donationService, ILockService lockService, IOptions<DonateCraftOptions> options, ICharityService charityService, ILogger<CallbackController> logger)
    {
        this._client = client;
        this._donationService = donationService;
        this._lockService = lockService;
        this._charityService = charityService;
        this._logger = logger;
        this._apiKey = options.Value.JustGivingApiKey;
        this._donateCraftUi = options.Value.DonateCraftUiUrl;
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string data)
    {
        if (data == null)
        {
            this._logger.LogWarning("No data received from callback initiator {Data}, error code 1", data);
            return Redirect($"{this._donateCraftUi}?status=error&code=1");
        }
        var justGivingData = data.Split("~");
        var donationId = justGivingData[DONATION_ID];
        if (justGivingData.Length < 2)
        {
            this._logger.LogWarning("Data received is malformed {Data}, error code 2", data);
            return Redirect($"{this._donateCraftUi}?status=error&code=2");
        }
        var player = justGivingData[PLAYER_ID];
        var paidForKey = justGivingData.Length > 2 ? justGivingData[DONOR_ID] : null;
        if (string.IsNullOrWhiteSpace(donationId) || string.IsNullOrWhiteSpace(player))
        {
            this._logger.LogWarning("Donation id or player id are missing {Data}, error code 3", data);
            return Redirect($"{this._donateCraftUi}?status=error&code=3");
        }

        Lock currentLock = null;
        try
        {
            currentLock = await this._lockService.GetById(player);
        }
        catch (ResourceNotFoundException)
        {
            this._logger.LogWarning("Lock with id {Player} not found, error code 4", player);
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
            this._logger.LogInformation("Donation was not successful! Status is {Status}, error code 5", justGivingDonation?.Status);
            return Redirect($"{this._donateCraftUi}?status=error&code=5");
        }

        var charityData = await GetCharityData(justGivingDonation.CharityId);
        var name = charityData.Name;
        var id = justGivingDonation.CharityId;
        
        
        await this._donationService.Create(player, new Donation
        {
            Amount = Convert.ToDouble(justGivingDonation.Amount),
            Id = justGivingDonation.Id.ToString(),
            CharityId = id,
            CharityName = name,
            CreatedDate = DateTime.Now,
            PaidForId = paidForKey ?? player,
            Private = string.IsNullOrWhiteSpace(justGivingDonation.Amount)
        });
        var charity = await this._charityService.GetById(id.ToString());
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

    private async Task<JustGivingFundraiser> GetFundraiserData(string pageShortName)
    {
        var fundraiserResponse = await this._client.GetAsync($"{this._apiKey}/v1/fundraising/pages/{pageShortName}");
        var fundraiserData = JsonSerializer.Deserialize<JustGivingFundraiser>(await fundraiserResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (!fundraiserResponse.IsSuccessStatusCode || fundraiserData == null)
        {
            throw new BadHttpRequestException($"Could not find a charity with id of {pageShortName}");
        }
        return fundraiserData;
    }
}