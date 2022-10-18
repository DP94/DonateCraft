using System.Collections;
using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public class PlayerControllerTest
{
    private IPlayerService _playerService;
    private PlayerController _controller;

    [SetUp]
    public void SetUp()
    {
        this._playerService = A.Fake<IPlayerService>();
        this._controller = new PlayerController(this._playerService, A.Fake<HttpContextAccessor>());
    }

    [Test]
    public async Task Get_ReturnsPlayers()
    {
        var players = new List<Player>
        {
            {
                new Player("abc", "def")
            },
            {
                new Player("ghi", "jkl")
            }
        };
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        var result = await this._controller.Get() as ObjectResult;
        CollectionAssert.AreEqual(players, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Get_ById_ReturnsPlayer()
    {
        var player = new Player
        {
            Id = "abc",
            Name = "def"
        };
        A.CallTo(() => this._playerService.GetPlayerById("abc")).Returns(player);
        var result = await this._controller.Get("abc") as ObjectResult;
        Assert.AreEqual(player, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Get_ById_Returns404_If_PlayerNull()
    {
        A.CallTo(() => this._playerService.GetPlayerById("1")).Returns((Player) null);
        var result = await this._controller.Get("1") as IStatusCodeActionResult;
        Assert.AreEqual(404, result.StatusCode);
    }

    [Test]
    public async Task Post_CreatesPlayer_Successfully()
    {
        var player = new Player
        {
            Id = "abc",
            Name = "def"
        };
        A.CallTo(() => this._playerService.CreatePlayer(player)).Returns(player);
        var result = await this._controller.Post(player) as ObjectResult;
        Assert.AreEqual(player, result.Value);
        Assert.AreEqual(201, result.StatusCode);
    }
    
    [Test]
    public async Task Delete_DeletesPlayer_Successfully()
    {
        var result = await this._controller.Delete("abc") as StatusCodeResult;
        Assert.AreEqual(204, result.StatusCode);
    }
}