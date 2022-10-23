using System.Collections;
using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public class DeathControllerTest
{

    private IDeathService _deathService;
    private DeathController _controller;

    [SetUp]
    public void SetUp()
    {
        this._deathService = A.Fake<IDeathService>();
        this._controller = new DeathController(this._deathService, A.Fake<HttpContextAccessor>());
    }

    [Test]
    public async Task Get_ReturnsPlayers()
    {
        var deaths = new List<Death>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
        A.CallTo(() => this._deathService.GetDeaths("test")).Returns(deaths);
        var result = await this._controller.GetDeaths("test") as ObjectResult;
        CollectionAssert.AreEqual(deaths, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Get_ById_ReturnsPlayer()
    {
        var death = new Death
        {
            Id = "abc",
            Reason = "def"
        };
        A.CallTo(() => this._deathService.GetDeathById("test", "abc")).Returns(death);
        var result = await this._controller.GetDeath("test", "abc") as ObjectResult;
        Assert.AreEqual(death, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Post_CreatesDeath_Successfully()
    {
        var death = new Death
        {
            Id = "abc",
            Reason = "def"
        };
        A.CallTo(() => this._deathService.CreateDeath("test", death)).Returns(death);
        var result = await this._controller.CreateDeath("test", death) as ObjectResult;
        Assert.AreEqual(death, result.Value);
        Assert.AreEqual(201, result.StatusCode);
    }
    
    [Test]
    public async Task Delete_DeletesDeath_Successfully()
    {
        var result = await this._controller.DeleteDeath("test", "abc") as StatusCodeResult;
        Assert.AreEqual(204, result.StatusCode);
    }
    
    [Test]
    public async Task Put_UpdatesDeath_Successfully()
    {
        var death = new Death
        {
            Id = "abc",
            Reason = "def"
        };
        A.CallTo(() => this._deathService.UpdateDeath("test", death)).Returns(death);
        var result = await this._controller.UpdateDeath("test", death) as ObjectResult;
        Assert.AreEqual(death, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
}