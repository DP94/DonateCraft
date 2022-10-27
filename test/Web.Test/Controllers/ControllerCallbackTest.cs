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
        this._client = new HttpClient(FakeHttpMessageHandler.GetHttpMessageHandler("{\"Status\": \"Accepted\"}", HttpStatusCode.OK));
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
    
    [Test]
    public async Task CallbackController_RedirectsToUi_WhenLockNotFound()
    {
        //FakeItEasy returns a blank proxy (but not null!) when stubs are not specified...
        A.CallTo(() => this._lockService.GetLock(A<string>.Ignored)).Returns((Lock)null);
        var result = await this._controller.Callback("1|5ba92742-af9d-4ad6-a5a7-c768dd9bc747") as RedirectResult;
        Assert.AreEqual("test.com", result.Url);
    }
    
        
    [Test]
    public async Task CallbackController_RedirectsToUi_WhenLock_AlreadyUnlocked()
    {
        A.CallTo(() => this._lockService.GetLock(A<string>.Ignored)).Returns(new Lock()
        {
            Unlocked = true
        });
        var result = await this._controller.Callback("1|5ba92742-af9d-4ad6-a5a7-c768dd9bc747") as RedirectResult;
        Assert.AreEqual("test.com", result.Url);
    }
    
    [Test]
    public async Task CallbackController_SuccessfullyCreatesDonation_AndUnlocksLock()
    {
        this._client = new HttpClient(FakeHttpMessageHandler.GetHttpMessageHandler(
            "{\"amount\":\"1.7441\",\"donationRef\":\"115070563\",\"id\":1500333570,\"status\":\"Accepted\",\"charityId\":2201, \"name\":\"Test\"}",
            HttpStatusCode.OK));
        this._client.BaseAddress = new Uri("http://justgiving.com");
        this._controller = new CallbackController(this._client, this._donationService, this._lockService, this._options);
        var theLock = new Lock { Id = "5ba92742-af9d-4ad6-a5a7-c768dd9bc747" };
        A.CallTo(() => this._lockService.GetLock(theLock.Id)).Returns(theLock);

        var result = await this._controller.Callback("1|5ba92742-af9d-4ad6-a5a7-c768dd9bc747") as RedirectResult;
        
        A.CallTo(() => this._donationService.Create("5ba92742-af9d-4ad6-a5a7-c768dd9bc747",
                A<Donation>.That.Matches(donation =>
                    donation.Amount == 1.7441 &&
                    donation.CharityName == "Test" &&
                    donation.CharityId == 2201 &&
                    donation.Id == "1500333570" &&
                    donation.PaidForId == "5ba92742-af9d-4ad6-a5a7-c768dd9bc747")))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => this._lockService.UpdateLock(A<Lock>.That.Matches(l => l.Unlocked == true && l.Id == theLock.Id && l.DonationId == "1500333570")))
            .MustHaveHappenedOnceExactly();

        Assert.AreEqual("test.com", result.Url);
    }
}