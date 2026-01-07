using System.Net.Http.Headers;
using System.Text.Json;
using Cloud.Services;
using Common.Exceptions;
using Common.Models;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Lock = Common.Models.Lock;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class CallbackController : ControllerBase
{
    private readonly string _donateCraftUi;
    private readonly ILogger<CallbackController> _logger;
    private readonly IRevivalQueueService _revivalQueueService;

    private const int DonationId = 0;
    private const int PlayerId = 1;
    private const int DonorId = 2;

    public CallbackController(IOptions<DonateCraftOptions> options, IRevivalQueueService revivalQueueService, ILogger<CallbackController> logger)
    {
        this._revivalQueueService = revivalQueueService;
        this._logger = logger;
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
        return Redirect($"{this._donateCraftUi}/players?status=success");
    }



}