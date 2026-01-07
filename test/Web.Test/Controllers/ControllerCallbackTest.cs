using System.Net;
using Cloud.Services;
using Common.Models;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Web.Controllers;
using Web.Test.Controllers.Fakes;
using Lock = Common.Models.Lock;

namespace Web.Test.Controllers;

[Ignore("Currently being refactored")]
public class ControllerCallbackTest
{

    private HttpClient _client;
    private CallbackController _controller;
    private IRevivalQueueService  _queueService;
    private IOptions<DonateCraftOptions> _options;
    private ILogger<CallbackController>  _logger;

    [SetUp]
    public void SetUp()
    {
        this._queueService  = A.Fake<IRevivalQueueService>();
        this._logger = A.Fake<ILogger<CallbackController>>();
        this._options = Options.Create(new DonateCraftOptions
        {
            DonateCraftUiUrl = "test.com",
            JustGivingApiKey = "123",
            JustGivingApiUrl = "justgiving.com"
        });
        this._controller = new CallbackController(this._options, this._queueService, this._logger);
    }

    [Test]
    public async Task CallbackController_ReturnsBadRequest_WhenJustGivingDoesntReturn_Exactly_2ValuesSeparatedBy_Delimiter()
    {
        var result = await this._controller.Callback("1500333570")  as RedirectResult;
        Assert.That("test.com?status=error&code=2", Is.EqualTo(result.Url));
    }
    
    [Test]
    public async Task CallbackController_ReturnsBadRequest_WhenJustGivingDoesntReturnPlayerId()
    {
        var result = await this._controller.Callback("1500333570~")  as RedirectResult;
        Assert.That("test.com?status=error&code=3", Is.EqualTo(result.Url));
    }
    
    [Test]
    public async Task CallbackController_ReturnsBadRequest_WhenJustGiving_DataMissing()
    {
        var result = await this._controller.Callback(null)  as RedirectResult;
        Assert.That("test.com?status=error&code=1", Is.EqualTo(result.Url));
    }
    
    [Test]
    public async Task CallbackController_ReturnsBadRequest_WhenJustGivingDoesntReturnDonationId()
    {
        var result = await this._controller.Callback("~5ba92742-af9d-4ad6-a5a7-c768dd9bc747") as RedirectResult;
        Assert.That("test.com?status=error&code=3", Is.EqualTo(result.Url));
    }
}