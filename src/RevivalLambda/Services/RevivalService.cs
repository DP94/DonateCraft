using System.Net.Http.Headers;
using System.Text.Json;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
using Microsoft.Extensions.Logging;
using Lock = Common.Models.Lock;

namespace RevivalLambda.Services;

public class RevivalService : IRevivalService
{
    private readonly IDonationService _donationService;
    private readonly ILockService _lockService;
    private readonly ICharityService _charityService;
    private readonly ILogger<RevivalService> _logger;
    private readonly HttpClient _client;
    private readonly string _apiKey;

    public RevivalService(IDonationService donationService, ILockService lockService, ICharityService charityService, ILogger<RevivalService> logger, HttpClient client)
    {
        this._donationService = donationService;
        this._lockService = lockService;
        this._charityService = charityService;
        this._logger = logger;
        this._client = client;
        this._apiKey =  Environment.GetEnvironmentVariable(Constants.JG_API_KEY);
    }

    public async Task ProcessRevival(RevivalMessage message)
    {
        var player = message.PlayerId;
        var donationId = message.DonationId;
        var paidForKey = message.PaidForById;

        Lock currentLock = null;
        try
        {
            currentLock = await this._lockService.GetById(player);
        }
        catch (ResourceNotFoundException)
        {
            throw new InvalidOperationException($"Lock with id {player} not found, error code 4");
        }
        if (currentLock.Status == LockStatus.Unlocked)
        {
            return;
        }

        this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var justGivingDonation = await this.GetDonationData(donationId);
        if (justGivingDonation is not { Status: "Accepted" or "Pending" })
        {
            //Send error back here
            this._logger.LogInformation("Donation was not successful! Status is {Status}, error code 5",
                justGivingDonation?.Status);
            currentLock.Status = LockStatus.Error;
            await this._lockService.Update(currentLock);
            return; //Redirect($"{this._donateCraftUi}?status=error&code=5");
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
        currentLock.Status = LockStatus.Unlocked;
        await this._lockService.Update(currentLock);
        this._logger.LogInformation("Successfully unlocked player {Player}", player);
    }


    private async Task<JustGivingDonation> GetDonationData(string donationId)
    {
        for (var i = 0; i < 10; i++)
        {
            var donationData = await this._client.GetAsync($"{this._apiKey}/v1/donation/{donationId}");
            if (!donationData.IsSuccessStatusCode)
            {
                this._logger.LogWarning("Get donation {DonationId} data did not succeed {DonationDataStatusCode}", donationId,
                    donationData.StatusCode);
                await Task.Delay(TimeSpan.FromMilliseconds(500) * i);
                continue;
            }
            donationData.EnsureSuccessStatusCode();
            var responseBody = await donationData.Content.ReadAsStringAsync();
            var justGivingDonation = JsonSerializer.Deserialize<JustGivingDonation>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!donationData.IsSuccessStatusCode || justGivingDonation == null)
            {
                throw new InvalidOperationException($"Could not find a donation with id of {donationId}");
            }
            return justGivingDonation;
        }
        throw new InvalidOperationException("Donation not found");
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
            throw new InvalidOperationException($"Could not find a charity with id of {charityId}");
        }
        return charityData;
    }
}