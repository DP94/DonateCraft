using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Services.Aws;
using Cloud.Util;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Lock = Common.Models.Lock;
using ResourceNotFoundException = Common.Exceptions.ResourceNotFoundException;

namespace Cloud.Test.Services;

public class LockDynamoDbCloudServiceTest
{
    private ILockCloudService _cloudService;
    private IDonationCloudService _donationCloudService;
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
            LockTableName = "Lock"
        });
        this._playerCloudService = new PlayerDynamoDbCloudService(this._dynamoDb, options);
        this._donationCloudService = new DonationDynamoDbCloudService(this._playerCloudService);
        this._cloudService = new LockDynamoDbCloudService(this._dynamoDb, this._donationCloudService, this._playerCloudService, A.Fake<IMemoryCache>(), options);
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
        Assert.That(locks.Count, Is.EqualTo(retrievedLocks.Count));
    }
    
    [Test]
    public async Task CreateLock_SuccessfullyCreatesLock()
    {
        var newLock = CreateLock();
        await this._cloudService.Create(newLock);
        var retrievedLock = await GetLock(newLock.Id);
        Assert.That(newLock.Id, Is.EqualTo(retrievedLock.Id));
        Assert.That(newLock.Unlocked, Is.EqualTo(retrievedLock.Unlocked));
    }
    
    [Test]
    public async Task CreateLock_ThatAlreadyExists_ThrowsResourceAlreadyExistsException()
    {
        var charity = CreateLock();
        await this._cloudService.Create(charity);
        Assert.ThrowsAsync<ResourceExistsException>(() => this._cloudService.Create(charity));
    }
    
    [Test]
    public async Task GetLock_SuccessfullyGetsLock()
    {
        var newLock = CreateLock();
        await this._cloudService.Create(newLock);
        var retrievedLock = await this._cloudService.GetLock(newLock.Id);
        Assert.That(newLock.Id, Is.EqualTo(retrievedLock.Id));
        Assert.That(newLock.Unlocked, Is.EqualTo(retrievedLock.Unlocked));
    }

    [Test]
    public async Task GetLockWithPlayerId_SuccessfullyReturns_OnlyTheCorrectLock()
    {
        var lock1 = await this._cloudService.Create(CreateLock());
        var lock2 = await this._cloudService.Create(CreateLock());
        var locks = await this._cloudService.GetLocksForPlayers(new List<string> { lock1.Id });
        var retrievedLock = locks[0];
        Assert.That(1, Is.EqualTo(locks.Count));
        Assert.That(lock1.Id, Is.EqualTo(retrievedLock.Id));
        Assert.That(lock2.Id, Is.Not.EqualTo(retrievedLock.Id));
    }
    
    [Test]
    public async Task GetLockWithPlayerId_SuccessfullyReturns_AllCorrectLocks()
    {
        var lock1 = await this._cloudService.Create(CreateLock());
        var lock2 = await this._cloudService.Create(CreateLock());
        var locks = await this._cloudService.GetLocksForPlayers(new List<string>
        {
            lock1.Id, lock2.Id
        });
        Assert.That(2, Is.EqualTo(locks.Count));
    }
    
    [Test]
    public async Task GetLockWithPlayerId_SuccessfullyReturns_NoLocks_IfNoLocksSpecified()
    {
        var locks = await this._cloudService.GetLocksForPlayers(new List<string>());
        Assert.That(0, Is.EqualTo(locks.Count));
    }

    [Test]
    public async Task GetLockWithPlayerId_SuccessfullyReturns_Donations_ForPlayer()
    {
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test"
        };
        var donation = new Donation
        {
            Id = Guid.NewGuid().ToString(),
            Amount = 1,
            CharityId = 1,
            CharityName = "test",
            PaidForId = player.Id
        };
        var lock1 = CreateLock();
        lock1.Id = player.Id;
        lock1.DonationId = donation.Id;
        await this._cloudService.Create(lock1);
        await this._playerCloudService.CreatePlayer(player);
        await this._donationCloudService.Create(player.Id, donation);

        var result = await this._cloudService.GetLocksForPlayers(new List<string> { lock1.Id });
        var retrievedDonation = result[0].Donation;
        Assert.That(donation.Id, Is.EqualTo(retrievedDonation.Id));
        Assert.That(donation.Amount, Is.EqualTo(retrievedDonation.Amount));
        Assert.That(donation.CharityId, Is.EqualTo(retrievedDonation.CharityId));
        Assert.That(donation.CharityName, Is.EqualTo(retrievedDonation.CharityName));
        Assert.That(donation.PaidForId, Is.EqualTo(retrievedDonation.PaidForId));
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
        Assert.That(retrievedLock, Is.Not.Null);

        await this._cloudService.DeleteLock(newLock.Id);
        retrievedLock = await GetLock(newLock.Id);
        Assert.That(retrievedLock, Is.Null);
    }
    
    
    [Test]
    public void UpdateLock_ThatDoesntExist_ThrowsResourceNotFoundException()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            this._cloudService.UpdateLock(new Lock(Guid.NewGuid().ToString(), false)));
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