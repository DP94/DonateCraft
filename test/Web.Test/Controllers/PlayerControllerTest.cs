using System.Collections;
using Cloud.Services;
using Common.Exceptions;
using Common.Models;
using Core.Services;
using Core.Services.Lock;
using Core.Services.Player;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
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
        this._controller = new PlayerController(this._playerService, this._lockService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetAll_ReturnsPlayers()
    {
        var players = new List<Player>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
        A.CallTo(() => this._playerService.GetAll()).Returns(players);
        var result = await this._controller.GetAll() as ObjectResult;
        CollectionAssert.AreEqual(players, (IEnumerable)result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test] public async Task GetAll_ReturnsPlayers_AndSetsIsDead()
    {
        var players = new List<Player>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
        A.CallTo(() => this._playerService.GetAll()).Returns(players);
        A.CallTo(() => this._lockService.GetAll()).Returns(new List<Lock>()
        {
            new("ghi", true)
        });

        var result = await this._controller.GetAll() as ObjectResult;
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
        A.CallTo(() => this._playerService.GetById("abc")).Returns(player);
        var result = await this._controller.GetById("abc") as ObjectResult;
        Assert.AreEqual(player, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Get_ById_Returns404_If_PlayerNull()
    {
        A.CallTo(() => this._playerService.GetById("1")).Throws<ResourceNotFoundException>();
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._controller.GetById("1"));
    }

    [Test]
    public async Task Create_CreatesPlayer_Successfully()
    {
        var player = new Player
        {
            Id = "abc",
            Name = "def"
        };
        A.CallTo(() => this._playerService.Create(player)).Returns(player);
        var result = await this._controller.Create(player) as ObjectResult;
        Assert.AreEqual(player, result.Value);
        Assert.AreEqual(201, result.StatusCode);
    }

    [Test]
    public async Task Create_Returns400_WhenPlayerId_NotPresent()
    {
        A.CallTo(() => this._playerService.Create(A<Player>.Ignored)).Throws<ResourceExistsException>();
        var result = await this._controller.Create(new Player()) as ObjectResult;
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
    public async Task Update_UpdatesPlayer_Successfully()
    {
        var player = new Player
        {
            Id = "abc",
            Name = "def"
        };
        A.CallTo(() => this._playerService.Update(player)).Returns(player);
        var result = await this._controller.Update(player) as ObjectResult;
        Assert.AreEqual(player, result.Value);
        Assert.AreEqual(200, result.StatusCode);
    }
}