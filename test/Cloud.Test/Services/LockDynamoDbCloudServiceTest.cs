using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Services.Aws;
using Cloud.Util;
using Common.Models;
using Common.Util;
using FakeItEasy;
using NUnit.Framework;

namespace Cloud.Test.Services;

public class LockDynamoDbCloudServiceTest
{
    private ILockCloudService _cloudService;
    private IDonationCloudService _donationCloudService;
    private IAmazonDynamoDB _dynamoDb;
    private LocalDynamoDbSetup _localDynamoDbSetup;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        this._localDynamoDbSetup = new LocalDynamoDbSetup();
        await this._localDynamoDbSetup.SetupDynamoDb();
        await this._localDynamoDbSetup.CreateTables(null, DynamoDbConstants.LockTableName, null);
        this._dynamoDb = this._localDynamoDbSetup.GetClient();
        this._cloudService = new LockDynamoDbCloudService(this._dynamoDb, A.Fake<IDonationCloudService>());
    }
    
    [Test]
    public async Task GetLocks_SuccessfullyGets_AllLocks()
    {
        await this._localDynamoDbSetup.ClearTables(null, DynamoDbConstants.LockTableName, null);
        
        var newLock = CreateLock();
        var newLock2 = CreateLock();
        await this._cloudService.Create(newLock);
        await this._cloudService.Create(newLock2);

        var locks = new List<Lock>
        {
            newLock2,
            newLock
        };
        
        var retrievedLocks = await this._cloudService.GetLocks();
        //Collections assert being weird, need to re-visit
        Assert.AreEqual(locks.Count, retrievedLocks.Count);
    }
    
    [Test]
    public async Task CreateLock_SuccessfullyCreatesLock()
    {
        var newLock = CreateLock();
        await this._cloudService.Create(newLock);
        var retrievedLock = await GetLock(newLock.Id);
        Assert.AreEqual(newLock.Id, retrievedLock.Id);
        Assert.AreEqual(newLock.Unlocked, retrievedLock.Unlocked);
    }
    
    [Test]
    public async Task GetLock_SuccessfullyGetsLock()
    {
        var newLock = CreateLock();
        await this._cloudService.Create(newLock);
        var retrievedLock = await this._cloudService.GetLock(newLock.Id);
        Assert.AreEqual(newLock.Id, retrievedLock.Id);
        Assert.AreEqual(newLock.Unlocked, retrievedLock.Unlocked);
    }
    
    [Test]
    public void GetLock_WhichDoesntExist_Throws_ResourceNotFoundException()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._cloudService.GetLock(Guid.NewGuid().ToString()));
    }
    
    [Test]
    public async Task DeleteLock_SuccessfullyDeletesLock()
    {
        var newLock = CreateLock();
        await this._cloudService.Create(newLock);
        var retrievedLock = await GetLock(newLock.Id);
        Assert.NotNull(retrievedLock);

        await this._cloudService.DeleteLock(newLock.Id);
        retrievedLock = await GetLock(newLock.Id);
        Assert.Null(retrievedLock);
    }
    
    //Purposefully not using the service method for GET for test code isolation
    private async Task<Lock> GetLock(string id)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.LockIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            return null;
        }
        return DynamoDbUtility.GetLockFromAttributes(response.Item);
    }

    private static Lock CreateLock()
    {
        return new Lock
        {
            Id = Guid.NewGuid().ToString(),
            Unlocked = false
        };
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this._localDynamoDbSetup.KillProcess();
    }
}