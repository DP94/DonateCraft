using Common.Exceptions;
using Common.Models;
using Core.Services.Lock;
using Core.Services.Player;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Web.Controllers;
using Lock = Common.Models.Lock;

namespace Web.Test.Controllers;

public class PlayerWithIdControllerTest : AbstractWithIdControllerTest<PlayerController, Player, IPlayerService>
{
    private ILockService _lockService;
    
    [Test] 
    public async Task GetAll_ReturnsPlayers_AndSetsIsDead()
    {
        var players = new List<Player>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
        A.CallTo(() => this._service.GetAll()).Returns(players);
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
    public async Task Create_Returns400_WhenPlayerId_NotPresent()
    {
        A.CallTo(() => this._service.Create(A<Player>.Ignored)).Throws<ResourceExistsException>();
        var result = await this._controller.Create(new Player()) as ObjectResult;
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Player id must be supplied when creating a player", result.Value);
    }

    protected override IPlayerService CreateServiceFake()
    {
        this._service = A.Fake<IPlayerService>();
        return this._service;
    }

    protected override PlayerController CreateController(IPlayerService service)
    {
        this._lockService = A.Fake<ILockService>();
        return new PlayerController(service, this._lockService);
    }

    protected override Player CreateData()
    {
        return new Player(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
    }
}