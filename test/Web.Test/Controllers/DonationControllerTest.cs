using System.Collections;
using Common.Models;
using Core.Services;
using Core.Services.Donation;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public class DonationControllerTest
{
    private IDonationService _donationService;
    private DonationController _controller;

    [SetUp]
    public void SetUp()
    {
        this._donationService = A.Fake<IDonationService>();
        this._controller = new DonationController(this._donationService, A.Fake<HttpContextAccessor>());
    }

    [Test]
    public async Task Get_ReturnsPlayers()
    {
        var donations = new List<Donation>
        {
            new("abc", 1, DateTime.Now, 1, "def", Guid.NewGuid().ToString(), false),
            new("abc", 2, DateTime.Now, 2, "def", Guid.NewGuid().ToString(), false),
        };
        A.CallTo(() => this._donationService.GetDonations("test")).Returns(donations);
        var result = await this._controller.GetDonations("test") as ObjectResult;
        CollectionAssert.AreEqual(donations, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Get_ById_ReturnsPlayer()
    {
        var donation = new Donation(Guid.NewGuid().ToString(), 1, DateTime.Now, 1, "def", Guid.NewGuid().ToString(), false);
        A.CallTo(() => this._donationService.GetDonation("test", donation.Id)).Returns(donation);
        var result = await this._controller.GetDonation("test", donation.Id) as ObjectResult;
        Assert.AreEqual(donation, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Post_CreatesDonation_Successfully()
    {
        var donation = new Donation(Guid.NewGuid().ToString(), 1, DateTime.Now, 1, "def", Guid.NewGuid().ToString(), false);
        A.CallTo(() => this._donationService.Create("test", donation)).Returns(donation);
        var result = await this._controller.CreateDonation("test", donation) as ObjectResult;
        Assert.AreEqual(donation, result.Value);
        Assert.AreEqual(201, result.StatusCode);
    }
    
    [Test]
    public async Task Delete_DeletesDonation_Successfully()
    {
        var result = await this._controller.DeleteDonation("test", "abc") as StatusCodeResult;
        Assert.AreEqual(204, result.StatusCode);
    }
    
    [Test]
    public async Task Put_UpdatesDonation_Successfully()
    {
        var donation = new Donation(Guid.NewGuid().ToString(), 1, DateTime.Now, 1, "def", Guid.NewGuid().ToString(), false);
        A.CallTo(() => this._donationService.UpdateDonation("test", donation)).Returns(donation);
        var result = await this._controller.UpdateDonation("test", donation) as ObjectResult;
        Assert.AreEqual(donation, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
}