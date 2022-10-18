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
    private DeathController _controller;
    private IDeathService _deathService;

    [SetUp]
    public void SetUp()
    {
        this._deathService = A.Fake<IDeathService>();
        this._controller = new DeathController(this._deathService, A.Fake<HttpContextAccessor>());
    }

    [Test]
    public async Task GetDeaths_ReturnsCorrectDeaths()
    {
        var deaths = new List<Death>
        {
            new("abc", "def", "test", "dan"),
            new("ghi", "jkl", "test", "ronni")
        };
        A.CallTo(() => this._deathService.GetDeaths()).Returns(deaths);
        var result = await this._controller.GetDeaths() as ObjectResult;
        
        A.CallTo(() => this._deathService.GetDeaths()).MustHaveHappenedOnceExactly();
        CollectionAssert.AreEqual(deaths, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
    
    [Test]
    public async Task GetById_Returns_Correct_Value()
    {
        var death = new Death()
        {
            Id = "abc",
            PlayerId = "def",
            Reason = "Test"
        };
        A.CallTo(() => this._deathService.GetDeathById("abc")).Returns(death);
        var result = await this._controller.GetDeath("abc") as ObjectResult;
        A.CallTo(() => this._deathService.GetDeathById("abc")).MustHaveHappenedOnceExactly();
        Assert.AreEqual(death, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
    
    
    [Test]
    public async Task CreateDeath_Creates_Successfully()
    {
        var death = new Death
        {
            Id = "abc",
            PlayerId = "def",
            Reason = "test"
        };
        A.CallTo(() => this._deathService.CreateDeath(death)).Returns(death);
        var result = await this._controller.CreateDeath(death) as ObjectResult;
        A.CallTo(() => this._deathService.CreateDeath(death)).MustHaveHappenedOnceExactly();
        Assert.AreEqual(death, result.Value);
        Assert.AreEqual(201, result.StatusCode);
    }
    
    [Test]
    public async Task CreateDeath_Returns400_IfPlayerMissing() 
    {
        var death = new Death
        {
            Id = "abc",
            Reason = "test"
        };
        A.CallTo(() => this._deathService.CreateDeath(death)).Returns(death);
        var result = await this._controller.CreateDeath(death) as ObjectResult;
        A.CallTo(() => this._deathService.CreateDeath(death)).MustNotHaveHappened();
        Assert.AreEqual(400, result.StatusCode);
    }
    
    [Test]
    public async Task UpdateDeath_Updates_Successfully()
    {
        var death = new Death
        {
            Id = "123",
            PlayerId = "def",
            Reason = "test"
        };
        var existingDeath = new Death
        {
            Id = "123",
            PlayerId = "def"
        };
        A.CallTo(() => this._deathService.UpdateDeath(death)).Returns(death);
        A.CallTo(() => this._deathService.GetDeathById(existingDeath.Id)).Returns(existingDeath);
        var result = await this._controller.UpdateDeath(death) as ObjectResult;
        A.CallTo(() => this._deathService.UpdateDeath(death)).MustHaveHappenedOnceExactly();
        Assert.AreEqual(death, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
    
    [Test]
    public async Task UpdateDeath_Returns400_IfPlayerId_DoesNotMatchExisting() 
    {
        var death = new Death
        {
            Id = "abc",
            PlayerId = "test1"
        };
        var existingDeath = new Death
        {
            Id = "abc",
            PlayerId = "different"
        };
        A.CallTo(() => this._deathService.UpdateDeath(death)).Returns(death);
        A.CallTo(() => this._deathService.GetDeathById(existingDeath.Id)).Returns(existingDeath);
        var result = await this._controller.UpdateDeath(death) as ObjectResult;
        A.CallTo(() => this._deathService.UpdateDeath(death)).MustNotHaveHappened();
        Assert.AreEqual(400, result.StatusCode);
    }
    
    [Test]
    public async Task DeleteDeath_Deletes_Successfully()
    {
        var result = await this._controller.DeleteDeath("abc") as StatusCodeResult;
        Assert.AreEqual(204, result.StatusCode);
    }
}