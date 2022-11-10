using Common.Models;
using Core.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Web.Controllers;

namespace Web.Test.Controllers.Sorting;

public class PlayerControllerSortingTest : AbstractControllerSortingTest<Player, PlayerController>
{
    private IPlayerService _playerService;
    private ILockService _lockService;

    [Test]
    public async Task GetAll_Players_SortsByName_Successfully()
    {
        var players = CreatePlayers();
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("name")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.Get() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.AreEqual("jkl", newPlayers[0].Name);
        Assert.AreEqual("def", newPlayers[1].Name);
    }

    [Test]
    public async Task GetAll_Players_SortsById_Successfully()
    {
        var players = CreatePlayers();
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("id")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.Get() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.AreEqual("ghi", newPlayers[0].Id);
        Assert.AreEqual("abc", newPlayers[1].Id);
    }
    
    [Test]
    public async Task GetAll_Players_SortsByIdAsc_Successfully()
    {
        var players = CreatePlayers();
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("id")},
            { "sortOrder", new StringValues("asc")}
        });
        var result = await this._controller.Get() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.AreEqual("abc", newPlayers[0].Id);
        Assert.AreEqual("ghi", newPlayers[1].Id);
    }
    
    [Test]
    public async Task GetAll_Players_SortsByIdDeathCountDesc_Successfully()
    {
        var players = CreatePlayers();
        players[0].Deaths = new List<Death>
        {
            new("1", "Unit tests")
        };
        players[1].Deaths = new List<Death>
        {
            new("2", "More unit tests"), new("3", "Too many unit tests")
        };
        
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("deaths")},
            { "sortOrder", new StringValues("desc")}
        });
        var result = await this._controller.Get() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.AreEqual("ghi", newPlayers[0].Id);
        Assert.AreEqual("abc", newPlayers[1].Id);
    }
    
    [Test]
    public async Task GetAll_Players_SortsByIdDeathCountAsc_Successfully()
    {
        var players = CreatePlayers();
        players[0].Deaths = new List<Death>
        {
            new("1", "Unit tests")
        };
        players[1].Deaths = new List<Death>
        {
            new("2", "More unit tests"), new("3", "Too many unit tests")
        };
        
        A.CallTo(() => this._playerService.GetPlayers()).Returns(players);
        SetQueryParams(new Dictionary<string, StringValues>
        {
            { "sortBy", new StringValues("deaths")},
            { "sortOrder", new StringValues("asc")}
        });
        var result = await this._controller.Get() as ObjectResult;
        var newPlayers = result.Value as List<Player>;
        Assert.AreEqual("abc", newPlayers[0].Id);
        Assert.AreEqual("ghi", newPlayers[1].Id);
    }
    

    private static List<Player> CreatePlayers()
    {
        return new List<Player>
        {
            new("abc", "def"),
            new("ghi", "jkl")
        };
    }
    
    protected override PlayerController CreateController()
    {
        this._playerService = A.Fake<IPlayerService>();
        this._lockService = A.Fake<ILockService>();
        return new PlayerController(this._playerService, this._lockService);
    }
}