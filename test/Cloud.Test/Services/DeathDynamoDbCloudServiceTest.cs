using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Services.Aws;
using Cloud.Util;
using Common.Models;
using Common.Util;
using NUnit.Framework;

namespace Cloud.Test.Services;

public class DeathDynamoDbCloudServiceTest
{
    private IDeathCloudService _deathCloudService;
    private ILockCloudService _lockCloudService;
    private IPlayerCloudService _playerCloudService;
    private IAmazonDynamoDB _dynamoDb;
    private LocalDynamoDbSetup _localDynamoDbSetup;
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        this._localDynamoDbSetup = new LocalDynamoDbSetup();
        await this._localDynamoDbSetup.SetupDynamoDb();
        await this._localDynamoDbSetup.CreateTables(DynamoDbConstants.PlayerTableName, DynamoDbConstants.LockTableName, null);
        this._dynamoDb = this._localDynamoDbSetup.GetClient();
        this._playerCloudService = new PlayerDynamoDbCloudService(this._dynamoDb);
        this._lockCloudService = new LockDynamoDbCloudService(this._dynamoDb);
        this._deathCloudService = new DeathDynamoDbStorageService(this._playerCloudService, this._lockCloudService);
    }

    [Test]
    public async Task GetDeath_SuccessfullyGetsPlayerDeaths()
    {
        var playerId = Guid.NewGuid().ToString();
        var deathId = Guid.NewGuid().ToString();
        await this.CreateDeathForPlayer(playerId, deathId);
        var death = await this._deathCloudService.GetDeathById(playerId, deathId);
        Assert.AreEqual(deathId, death.Id);
        Assert.AreEqual("Test", death.Reason);
        Assert.AreEqual(playerId, death.PlayerId);
    }
    
    [Test]
    public void GetDeath_ThrowsException_IfPlayerNotFound()
    {
         Assert.ThrowsAsync<ResourceNotFoundException>(() => this._deathCloudService.GetDeathById("test", ""));
    }
    
    [Test]
    public async Task GetDeath_ThrowsException_IfDeathNotFound()
    {
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Dan"
        };
        await this._playerCloudService.CreatePlayer(player);
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._deathCloudService.GetDeathById(player.Id, ""));
    }
    
    [Test]
    public async Task GetDeaths_SuccessfullyGetsAllPlayerDeaths()
    {
        var death = new Death(Guid.NewGuid().ToString(), "Test");
        var secondDeath = new Death(Guid.NewGuid().ToString(), "Test");
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Deaths = new List<Death>
            {
                death, secondDeath
            }
        };
        await this._playerCloudService.CreatePlayer(player);
        var deaths = await this._deathCloudService.GetDeaths(player.Id);
        Assert.AreEqual(2, deaths.Count);
    }
    
    [Test]
    public void GetDeaths_ThrowsException_IfPlayerNotFound()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._deathCloudService.GetDeaths("test"));
    }
    

    [Test]
    public async Task CreateDeath_SuccessfullyAddsToPlayer()
    {
        var playerId = Guid.NewGuid().ToString();
        var deathId = Guid.NewGuid().ToString();
        await this.CreateDeathForPlayer(playerId, deathId);
        var player = await this._playerCloudService.GetPlayerById(playerId);
        var death = await this._deathCloudService.GetDeathById(playerId, deathId);
        
        Assert.AreEqual(1, player.Deaths.Count);

        var savedDeath = player.Deaths.First();
        Assert.AreEqual(death.Id, savedDeath.Id);
        Assert.AreEqual(death.Reason, savedDeath.Reason);
        Assert.AreEqual(player.Id, savedDeath.PlayerId);
    }
    
    [Test]
    public async Task CreateDeath_SuccessfullyCreatesLock()
    {
        var playerId = Guid.NewGuid().ToString();
        var deathId = Guid.NewGuid().ToString();
        await this.CreateDeathForPlayer(playerId, deathId);
        var theLock = await this._lockCloudService.GetLockByKey(playerId);
        Assert.AreEqual(playerId, theLock.Key);
        Assert.False(theLock.Unlocked);
    }
    
    [Test]
    public async Task UpdateDeath_SuccessfullyUpdatesDeath()
    {
        var playerId = Guid.NewGuid().ToString();
        var deathId = Guid.NewGuid().ToString();
        await this.CreateDeathForPlayer(playerId, deathId);
        var player = await this._playerCloudService.GetPlayerById(playerId);
        var death = player.Deaths.First();
        death.Reason = "Updated reason";

        await this._deathCloudService.UpdateDeath(player.Id, death);
        player = await this._playerCloudService.GetPlayerById(player.Id);
        death = player.Deaths.First();
        
        Assert.AreEqual("Updated reason", death.Reason);
    }
    
    [Test]
    public async Task DeleteDeath_SuccessfullyDeletesDeathFromPlayer()
    {
        var playerId = Guid.NewGuid().ToString();
        var deathId = Guid.NewGuid().ToString();
        await this.CreateDeathForPlayer(playerId, deathId);
        await this._deathCloudService.DeleteDeath(playerId, deathId);
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._deathCloudService.GetDeathById(playerId, deathId));
    }

    private async Task CreateDeathForPlayer(string playerId, string deathId)
    {
        var player = new Player
        {
            Id = playerId,
            Name = "Dan"
        };
        var death = new Death
        {
            Id = deathId,
            Reason = "Test",
            CreatedDate = DateTime.Now,
            PlayerId = playerId
        };
        await this._playerCloudService.CreatePlayer(player);
        await this._deathCloudService.CreateDeath(player.Id, death);
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        this._localDynamoDbSetup.KillProcess();
    }
}