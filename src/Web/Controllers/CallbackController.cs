using System.Net.Http.Headers;
using System.Text.Json;
using Cloud.Services;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
using Core.Services.Player;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Lock = Common.Models.Lock;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class CallbackController : ControllerBase
{
    private const string CreditsMarker = "credits";

    private readonly string _donateCraftUi;
    private readonly ILogger<CallbackController> _logger;
    private readonly IRevivalQueueService _revivalQueueService;
    private readonly ILockService _lockService;
    private readonly IPlayerService _playerService;
    private readonly HttpClient _httpClient;
    private readonly string _justGivingApiKey;

    private const int DonationId = 0;
    private const int PlayerId = 1;
    private const int DonorId = 2;

    public CallbackController(IOptions<DonateCraftOptions> options, IRevivalQueueService revivalQueueService, ILockService lockService, IPlayerService playerService, HttpClient httpClient, ILogger<CallbackController> logger)
    {
        this._revivalQueueService = revivalQueueService;
        this._logger = logger;
        this._lockService = lockService;
        this._playerService = playerService;
        this._httpClient = httpClient;
        this._donateCraftUi = options.Value.DonateCraftUiUrl;
        this._justGivingApiKey = options.Value.JustGivingApiKey ?? Environment.GetEnvironmentVariable(Constants.JG_API_KEY);
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

        if (justGivingData.Length > 0 && justGivingData[0] == CreditsMarker)
        {
            return await this.HandleCreditPurchase(justGivingData, data);
        }

        var donationId = justGivingData[DonationId];
        if (justGivingData.Length < 2)
        {
            this._logger.LogWarning("Data received is malformed {Data}, error code 2", data);
            return Redirect($"{this._donateCraftUi}?status=error&code=2");
        }
        var player = justGivingData[PlayerId];
        var paidForKey = justGivingData.Length > 2 ? justGivingData[DonorId] : null;
        if (string.IsNullOrWhiteSpace(donationId) || string.IsNullOrWhiteSpace(player))
        {
            this._logger.LogWarning("Donation id or player id are missing {Data}, error code 3", data);
            return Redirect($"{this._donateCraftUi}?status=error&code=3");
        }

        await this._revivalQueueService.Enqueue(new RevivalMessage
        {
            DonationId = donationId,
            PaidForById = paidForKey,
            PlayerId = player,
        });

        try
        {
            var currentLock = await this._lockService.GetById(player);
            currentLock.Status = LockStatus.Processing;
            await this._lockService.Update(currentLock);
        }
        catch (ResourceNotFoundException)
        {
            //In the event of someone donating when no lock is present
            return Redirect($"{this._donateCraftUi}?status=error&code=4");
        }

        return Redirect($"{this._donateCraftUi}/revivals?status=success");
    }

    private async Task<IActionResult> HandleCreditPurchase(string[] parts, string rawData)
    {
        if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[1]) || string.IsNullOrWhiteSpace(parts[2]))
        {
            this._logger.LogWarning("Credit purchase callback is malformed {Data}, error code 6", rawData);
            return Redirect($"{this._donateCraftUi}?status=error&code=6");
        }
        var donationId = parts[1];
        var playerId = parts[2];

        Common.Models.Player player;
        try
        {
            player = await this._playerService.GetById(playerId);
        }
        catch (ResourceNotFoundException)
        {
            this._logger.LogWarning("Credit purchase for unknown player {PlayerId}, error code 7", playerId);
            return Redirect($"{this._donateCraftUi}?status=error&code=7");
        }

        JustGivingDonation donation;
        try
        {
            donation = await this.GetDonation(donationId);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Could not fetch JustGiving donation {DonationId} for credit purchase, error code 8", donationId);
            return Redirect($"{this._donateCraftUi}?status=error&code=8");
        }

        if (donation is not { Status: "Accepted" or "Pending" } || !decimal.TryParse(donation.Amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            this._logger.LogInformation("Credit purchase donation not accepted, status {Status}, error code 9", donation?.Status);
            return Redirect($"{this._donateCraftUi}?status=error&code=9");
        }

        var earned = (int)Math.Floor(amount / Constants.CreditPriceGbp);
        var newBalance = Math.Min(Constants.MaxRevivalCredits, player.Credits + earned);
        var actuallyAdded = newBalance - player.Credits;
        player.Credits = newBalance;
        await this._playerService.Update(player);
        this._logger.LogInformation("Player {PlayerId} bought {Added} credits from £{Amount} donation (balance now {Balance}/{Max})", playerId, actuallyAdded, amount, newBalance, Constants.MaxRevivalCredits);

        return Redirect($"{this._donateCraftUi}/players?status=success&credits={actuallyAdded}");
    }

    private async Task<JustGivingDonation> GetDonation(string donationId)
    {
        this._httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        for (var i = 1; i <= 10; i++)
        {
            var response = await this._httpClient.GetAsync($"{this._justGivingApiKey}/v1/donation/{donationId}");
            if (!response.IsSuccessStatusCode)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500) * i);
                continue;
            }
            var body = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<JustGivingDonation>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed == null)
            {
                throw new InvalidOperationException($"Could not parse donation {donationId}");
            }
            return parsed;
        }
        throw new InvalidOperationException($"Donation {donationId} not found after retries");
    }
}