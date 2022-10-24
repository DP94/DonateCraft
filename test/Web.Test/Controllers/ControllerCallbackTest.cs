using System.Net;
using System.Text;
using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public class ControllerCallbackTest
{

    private HttpClient _client;
    private IDonationService _donationService;
    private ILockService _lockService;
    private CallbackController _controller;
    private IOptions<DonateCraftOptions> _options;

    [SetUp]
    public async Task SetUp()
    {
        this._client = new HttpClient(FakeHttpMessageHandler.GetHttpMessageHandler("", HttpStatusCode.OK));
        this._client.BaseAddress = new Uri("http://justgiving.com");
        this._donationService = A.Fake<IDonationService>();
        this._lockService = A.Fake<ILockService>();
        this._options = Options.Create(new DonateCraftOptions
        {
            DonateCraftUiUrl = "test.com",
            JustGivingApiKey = "123",
            JustGivingApiUrl = "justgiving.com"
        });
        this._controller = new CallbackController(this._client, this._donationService, this._lockService, this._options);
    }

    [Test]
    public async Task CallbackController_ReturnsBadRequest_WhenJustGivingDoesntReturn_Exactly_2ValuesSeparatedBy_Delimiter()
    {
        var result = await this._controller.Callback("1500333570") as ObjectResult;
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Invalid data returned from JustGiving!", result.Value);
    }
    
    [Test]
    public async Task CallbackController_ReturnsBadRequest_WhenJustGivingDoesntReturnPlayerId()
    {
        var result = await this._controller.Callback("1500333570|") as ObjectResult;
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Player or donation id is missing", result.Value);
    }
    
    [Test]
    public async Task CallbackController_ReturnsBadRequest_WhenJustGivingDoesntReturnDonationId()
    {
        var result = await this._controller.Callback("|5ba92742-af9d-4ad6-a5a7-c768dd9bc747") as ObjectResult;
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Player or donation id is missing", result.Value);
    }

    [Test]
    public async Task CallbackController_RedirectsToUi_WhenDonationIsNotSuccessOrPending()
    {
        this._client = new HttpClient(FakeHttpMessageHandler.GetHttpMessageHandler("{\"Status\": \"Failed\"}", HttpStatusCode.OK));
        this._client.BaseAddress = new Uri("http://justgiving.com");
        this._controller = new CallbackController(this._client, this._donationService, this._lockService, this._options);
        var result = await this._controller.Callback("1|5ba92742-af9d-4ad6-a5a7-c768dd9bc747") as RedirectResult;
        Assert.AreEqual("test.com", result.Url);
    }
}