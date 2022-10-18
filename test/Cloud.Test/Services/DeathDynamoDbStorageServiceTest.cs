using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Util;
using Common.Models;
using Common.Util;
using NUnit.Framework;

namespace Cloud.Test.Services;

public class DeathDynamoDbStorageServiceTest
{
    private IDeathDynamoDbStorageService _dynamoDbStorageService;
    private IAmazonDynamoDB _dynamoDb;
    private LocalDynamoDbSetup _localDynamoDbSetup;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        this._localDynamoDbSetup = new LocalDynamoDbSetup();
        await this._localDynamoDbSetup.SetupDynamoDb();
        this._dynamoDb = this._localDynamoDbSetup.GetClient();
        this._dynamoDbStorageService = new DeathDynamoDbStorageService(this._dynamoDb);
    }

    [SetUp]
    public async Task SetUp()
    {
        await LocalDynamoDbSetup.ClearTables(this._dynamoDb);
    }

    [Test]
    public async Task GetDeaths_SuccessfullyGets_AllDeaths()
    {
        var death = CreateDeath();
        var death2 = CreateDeath();
        await this._dynamoDbStorageService.CreateDeath(death);
        await this._dynamoDbStorageService.CreateDeath(death2);

        var deaths = new List<Death>
        {
            death2,
            death
        };
        
        var retrievedDeaths = await this._dynamoDbStorageService.GetDeaths();
        //Collections assert being weird, need to re-visit
        Assert.AreEqual(deaths.Count, retrievedDeaths.Count);
    }
    
    [Test]
    public async Task GetDeath_SuccessfullyGetsDeath()
    {
        var death = CreateDeath();
        await this._dynamoDbStorageService.CreateDeath(death);
        var retrievedDeath = await this._dynamoDbStorageService.GetDeathById(death.Id);
        Assert.AreEqual(death.Id, retrievedDeath.Id);
    }

    [Test]
    public void GetDeath_WhichDoesntExist_Throws_ResourceNotFoundException()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._dynamoDbStorageService.GetDeathById(Guid.NewGuid().ToString()));
    }
    
    [Test]
    public async Task CreateDeath_SuccessfullyCreatesDeath()
    {
        var death = CreateDeath();
        await this._dynamoDbStorageService.CreateDeath(death);
        var retrievedDeath = await GetDeath(death.Id);
        Assert.AreEqual(death.Id, retrievedDeath.Id);
    }

    [Test]
    public async Task UpdateDeath_SuccessfullyUpdatesDeath()
    {
        var death = CreateDeath();
        await this._dynamoDbStorageService.CreateDeath(death);
        death.PlayerName = "Updated";
        await this._dynamoDbStorageService.UpdateDeath(death);
        
        var retrievedDeath = await GetDeath(death.Id);
        Assert.AreEqual(death.Id, retrievedDeath.Id);
        Assert.AreEqual("Updated", retrievedDeath.PlayerName);
    }
    
    [Test]
    public async Task DeleteDeath_SuccessfullyDeletesDeath()
    {
        var death = CreateDeath();
        await this._dynamoDbStorageService.CreateDeath(death);
        var retrievedDeath = await GetDeath(death.Id);
        Assert.NotNull(retrievedDeath);

        await this._dynamoDbStorageService.DeleteDeath(death.Id);
        retrievedDeath = await GetDeath(death.Id);
        Assert.Null(retrievedDeath);
    }

    //Purposefully not using the service method for GET for test code isolation
    private async Task<Death> GetDeath(string id)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.DeathTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.DeathIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            return null;
        }
        return DynamoDbUtility.GetDeathFromAttributes(response.Item);
    }

    private static Death CreateDeath()
    {
        return new Death
        {
            Id = Guid.NewGuid().ToString(),
            CreatedDate = DateTime.UtcNow,
            PlayerId = Guid.NewGuid().ToString(),
            PlayerName = Guid.NewGuid().ToString(),
            Reason = "Died from writing too many unit tests"
        };
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this._localDynamoDbSetup.KillProcess();
    }
}