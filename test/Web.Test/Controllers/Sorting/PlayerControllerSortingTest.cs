using Common.Models;
using Core.Services.Lock;
using Core.Services.Player;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers.Sorting;

public class PlayerControllerSortingTest : AbstractControllerSortingTest<Player, PlayerController, IPlayerService>
{
    private IPlayerService _playerService;
    private ILockService _lockService;

    [Test]
    public async Task GetAll_Players_SortsByName_Successfully()
    {
        var players = CreateData();
        A.CallTo(() => this._playerService.GetAll()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("name")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.GetAll() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.That("jkl", Is.EqualTo(newPlayers[0].Name));
        Assert.That("def", Is.EqualTo(newPlayers[1].Name));
    }
    
    [Test]
    public async Task GetAll_Players_SortsByIdDeathCountDesc_Successfully()
    {
        var players = CreateData();
        players[0].Deaths = new List<Death>
        {
            new("1", "Unit tests")
        };
        players[1].Deaths = new List<Death>
        {
            new("2", "More unit tests"), new("3", "Too many unit tests")
        };
        
        A.CallTo(() => this._playerService.GetAll()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("deaths")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.GetAll() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.That("ghi", Is.EqualTo(newPlayers[0].Id));
        Assert.That("abc", Is.EqualTo(newPlayers[1].Id));
    }
    
    [Test]
    public async Task GetAll_Players_SortsByIdDeathCountAsc_Successfully()
    {
        var players = CreateData();
        players[0].Deaths = new List<Death>
        {
            new("1", "Unit tests")
        };
        players[1].Deaths = new List<Death>
        {
            new("2", "More unit tests"), new("3", "Too many unit tests")
        };
        
        A.CallTo(() => this._playerService.GetAll()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("deaths")},
            { "sortOrder", new StringValues("asc")}
        });
        var result = await this._controller.GetAll() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.That("abc", Is.EqualTo(newPlayers[0].Id));
        Assert.That("ghi", Is.EqualTo(newPlayers[1].Id));
    }

    protected override PlayerController CreateController(IPlayerService playerService) 
    {
        this._lockService = A.Fake<ILockService>();
        return new PlayerController(playerService, this._lockService);
    }

    protected override IPlayerService CreateServiceFake()
    {
        this._playerService = A.Fake<IPlayerService>();
        return this._playerService;
    }

    protected override List<Player> CreateData()
    {
        return new List<Player>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
    }
}