using System.Collections;
using Cloud.Services;
using Common.Exceptions;
using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers;

public class PlayerControllerTest
{
    private IPlayerService _playerService;
    private ILockService _lockService;
    private PlayerController _controller;

    [SetUp]
    public void SetUp()
    {
        this._playerService = A.Fake<IPlayerService>();
        this._lockService = A.Fake<ILockService>();
        this._controller = new PlayerController(this._playerService, this._lockService);
    }

    [Test]
    public async Task Get_ReturnsPlayers()
    {
        var players = new List<Player>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        var result = await this._controller.Get() as ObjectResult;
        CollectionAssert.AreEqual(players, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Get_ReturnsPlayers_AndSetsIsDead()
    {
        var players = new List<Player>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        A.CallTo(() => this._lockService.GetLocks()).Returns(new List<Lock>()
        {
            new("ghi", true)
        });

        var result = await this._controller.Get() as ObjectResult;
        var playersResult = result.Value as List<Player>;
        Assert.IsFalse(playersResult[0].IsDead);
        Assert.IsTrue(playersResult[1].IsDead);
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
        A.CallTo(() => this._playerService.GetPlayerById("1")).Throws<ResourceNotFoundException>();
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._controller.Get("1"));
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
    public async Task Post_Returns400_WhenPlayerId_NotPresent()
    {
        A.CallTo(() => this._playerService.CreatePlayer(A<Player>.Ignored)).Throws<ResourceExistsException>();
        var result = await this._controller.Post(new Player()) as ObjectResult;
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Player id must be supplied when creating a player", result.Value);
    }
    
    [Test]
    public async Task Delete_DeletesPlayer_Successfully()
    {
        var result = await this._controller.Delete("abc") as StatusCodeResult;
        Assert.AreEqual(204, result.StatusCode);
    }
    
    [Test]
    public async Task Put_UpdatesPlayer_Successfully()
    {
        var player = new Player
        {
            Id = "abc",
            Name = "def"
        };
        A.CallTo(() => this._playerService.UpdatePlayer(player)).Returns(player);
        var result = await this._controller.Put(player) as ObjectResult;
        Assert.AreEqual(player, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
}