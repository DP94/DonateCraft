using Amazon.DynamoDBv2;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Services.Aws;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
        var options = Options.Create(new DonateCraftOptions
        {
            DonateCraftUiUrl = "test.com",
            JustGivingApiKey = "123",
            JustGivingApiUrl = "justgiving.com",
            PlayerTableName = "Player",
            LockTableName = "Lock",
            CharityTableName = "Charity"
        });
        this._playerCloudService = new PlayerDynamoDbCloudService(this._dynamoDb, options);
        this._lockCloudService = new LockDynamoDbCloudService(this._dynamoDb, A.Fake<IDonationCloudService>(), A.Fake<IPlayerCloudService>(), A.Fake<IMemoryCache>(), options);
        this._deathCloudService = new DeathDynamoDbStorageService(this._playerCloudService, this._lockCloudService);
    }

    [Test]
    public async Task GetDeath_SuccessfullyGetsPlayerDeaths()
    {
        var playerId = Guid.NewGuid().ToString();
        var deathId = Guid.NewGuid().ToString();
        await this.CreateDeathForPlayer(playerId, deathId);
        var death = await this._deathCloudService.GetDeathById(playerId, deathId);
        Assert.That(deathId, Is.EqualTo(death.Id));
        Assert.That("Test", Is.EqualTo(death.Reason));
        Assert.That(playerId, Is.EqualTo(death.PlayerId));
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
        Assert.That(2, Is.EqualTo(deaths.Count));
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
        
        Assert.That(1, Is.EqualTo(player.Deaths.Count));

        var savedDeath = player.Deaths.First();
        Assert.That(death.Id, Is.EqualTo(savedDeath.Id));
        Assert.That(death.Reason, Is.EqualTo(savedDeath.Reason));
        Assert.That(player.Id, Is.EqualTo(savedDeath.PlayerId));
    }
    
    [Test]
    public async Task CreateDeath_SuccessfullyCreatesLock()
    {
        var playerId = Guid.NewGuid().ToString();
        var deathId = Guid.NewGuid().ToString();
        await this.CreateDeathForPlayer(playerId, deathId);
        var theLock = await this._lockCloudService.GetLock(playerId);
        Assert.That(playerId, Is.EqualTo(theLock.Id));
        Assert.That(theLock.Unlocked, Is.False);
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
        
        Assert.That("Updated reason", Is.EqualTo(death.Reason));
    }
    
    [Test]
    public void UpdateDeath_ThatDoesntExist_ThrowsResourceNotFoundException()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            this._deathCloudService.UpdateDeath(Guid.NewGuid().ToString(), new Death()));
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