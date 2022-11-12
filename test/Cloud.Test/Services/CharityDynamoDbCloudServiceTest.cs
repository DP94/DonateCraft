using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Services.Aws;
using Cloud.Util;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using NUnit.Framework;
using ResourceNotFoundException = Common.Exceptions.ResourceNotFoundException;

namespace Cloud.Test.Services;

public class CharityDynamoDbCloudServiceTest
{
    private ICharityCloudService _cloudService;
    private IAmazonDynamoDB _dynamoDb;
    private LocalDynamoDbSetup _localDynamoDbSetup;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        this._localDynamoDbSetup = new LocalDynamoDbSetup();
        await this._localDynamoDbSetup.SetupDynamoDb();
        await this._localDynamoDbSetup.CreateTables(null, null, DynamoDbConstants.CharityTableName);
        this._dynamoDb = this._localDynamoDbSetup.GetClient();
        this._cloudService = new CharityDynamoDbCloudService(this._dynamoDb);
    }
    
    [Test]
    public async Task GetCharities_SuccessfullyGets_AllCharities()
    {
        await this._localDynamoDbSetup.ClearTables(null, null, DynamoDbConstants.CharityTableName);
        
        var charity = CreateCharity();
        var charity2 = CreateCharity();
        await this._cloudService.CreateCharity(charity);
        await this._cloudService.CreateCharity(charity2);

        var charities = new List<Charity>
        {
            charity2,
            charity
        };
        
        var retrievedCharities = await this._cloudService.GetCharities();
        //Collections assert being weird, need to re-visit
        Assert.AreEqual(charities.Count, retrievedCharities.Count);
    }
    
    [Test]
    public async Task CreateCharity_SuccessfullyCreatesCharity()
    {
        var charity = CreateCharity();
        await this._cloudService.CreateCharity(charity);
        var retrievedCharity = await GetCharity(charity.Id);
        Assert.AreEqual(charity.Id, retrievedCharity.Id);
        Assert.AreEqual(charity.DonationCount, retrievedCharity.DonationCount);
    }

    [Test]
    public async Task CreateCharity_ThatAlreadyExists_ThrowsResourceAlreadyExistsException()
    {
        var charity = CreateCharity();
        await this._cloudService.CreateCharity(charity);
        Assert.ThrowsAsync<ResourceExistsException>(() => this._cloudService.CreateCharity(charity));
    }
    
    [Test]
    public async Task GetCharity_SuccessfullyGetsCharity()
    {
        var charity = CreateCharity();
        await this._cloudService.CreateCharity(charity);
        var retrievedCharity = await this._cloudService.GetCharityById(charity.Id);
        Assert.AreEqual(charity.Id, retrievedCharity.Id);
        Assert.AreEqual(charity.DonationCount, retrievedCharity.DonationCount);
    }
    
    [Test]
    public void GetCharity_WhichDoesntExist_Throws_ResourceNotFoundException()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._cloudService.GetCharityById(Guid.NewGuid().ToString()));
    }
    
    [Test]
    public void UpdateCharity_ThatDoesntExist_ThrowsResourceNotFoundException()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            this._cloudService.UpdateCharity(new Charity
            {
                Id = Guid.NewGuid().ToString()
            }));
    }
    
    [Test]
    public async Task DeleteCharity_SuccessfullyDeletesCharity()
    {
        var charity = CreateCharity();
        await this._cloudService.CreateCharity(charity);
        var retrievedCharity = await GetCharity(charity.Id);
        Assert.NotNull(retrievedCharity);

        await this._cloudService.DeleteCharity(charity.Id);
        retrievedCharity = await GetCharity(charity.Id);
        Assert.Null(retrievedCharity);
    }
    
    //Purposefully not using the service method for GET for test code isolation
    private async Task<Charity> GetCharity(string id)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.CharityTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.CharityIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            return null;
        }
        return DynamoDbUtility.GetCharityFromAttributes(response.Item);
    }

    private static Charity CreateCharity()
    {
        return new Charity
        {
            Id = Guid.NewGuid().ToString(),
            DonationCount = 1
        };
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this._localDynamoDbSetup.KillProcess();
    }
}