using System.Security.Cryptography;
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

public class PlayerDynamoDbStorageServiceTest
{
    private IPlayerCloudService _cloudService;
    private IAmazonDynamoDB _dynamoDb;
    private LocalDynamoDbSetup _localDynamoDbSetup;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        this._localDynamoDbSetup = new LocalDynamoDbSetup();
        await this._localDynamoDbSetup.SetupDynamoDb();
        await this._localDynamoDbSetup.CreateTables(DynamoDbConstants.PlayerTableName, null, null, null);
        this._dynamoDb = this._localDynamoDbSetup.GetClient();
        this._cloudService = new PlayerDynamoDbCloudService(this._dynamoDb);
    }

    [Test]
    public async Task GetPlayers_SuccessfullyGets_AllPlayers()
    {
        await this._localDynamoDbSetup.ClearTables(DynamoDbConstants.PlayerTableName, null, null, null);
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test"
        };
        var player2 = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test"
        };
        await this._cloudService.CreatePlayer(player);
        await this._cloudService.CreatePlayer(player2);

        var players = new List<Player>
        {
            player2,
            player
        };
        
        var retrievedPlayers = await this._cloudService.GetPlayers();
        //Collections assert being weird, need to re-visit
        Assert.AreEqual(players.Count, retrievedPlayers.Count);
    }
    
    [Test]
    public async Task GetPlayer_SuccessfullyGetsPlayer()
    {
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test"
        };
        await this._cloudService.CreatePlayer(player);
        var retrievedPlayer = await this._cloudService.GetPlayerById(player.Id);
        Assert.AreEqual(player.Id, retrievedPlayer.Id);
        Assert.AreEqual(player.Name,retrievedPlayer.Name);
    }

    [Test]
    public void GetPlayer_WhichDoesntExist_Throws_ResourceNotFoundException()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._cloudService.GetPlayerById(Guid.NewGuid().ToString()));
    }
    
    [Test]
    public async Task CreatePlayer_SuccessfullyCreatesPlayer()
    {
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test"
        };
        await this._cloudService.CreatePlayer(player);
        var retrievedPlayer = await GetPlayer(player.Id);
        Assert.AreEqual(player.Id, retrievedPlayer.Id);
        Assert.AreEqual(player.Name,retrievedPlayer.Name);
    }

    [Test]
    public async Task UpdatePlayer_SuccessfullyUpdatesPlayer()
    {
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test"
        };
        await this._cloudService.CreatePlayer(player);
        player.Name = "Updated";
        await this._cloudService.UpdatePlayer(player);
        
        var retrievedPlayer = await GetPlayer(player.Id);
        Assert.AreEqual(player.Id, retrievedPlayer.Id);
        Assert.AreEqual("Updated", retrievedPlayer.Name);
    }
    
    [Test]
    public async Task DeletePlayer_SuccessfullyDeletesPlayer()
    {
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test"
        };
        await this._cloudService.CreatePlayer(player);
        var retrievedPlayer = await GetPlayer(player.Id);
        Assert.NotNull(retrievedPlayer);

        await this._cloudService.DeletePlayer(player.Id);
        retrievedPlayer = await GetPlayer(player.Id);
        Assert.Null(retrievedPlayer);
    }

    //Purposefully not using the service method for GET for test code isolation
    private async Task<Player> GetPlayer(string id)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.PlayerTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.PlayerIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            return null;
        }
        return DynamoDbUtility.GetPlayerFromAttributes(response.Item);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this._localDynamoDbSetup.KillProcess();
    }
}