using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.DynamoDbLocal;
using Cloud.Services;
using Cloud.Services.Aws;
using Common.Models;
using Common.Util;
using NUnit.Framework;

namespace Cloud.Test.Services;

public class DonationDynamoDbCloudServiceTest
{
    private IDonationCloudService _donationCloudService;
    private IPlayerCloudService _playerCloudService;
    private IAmazonDynamoDB _dynamoDb;
    private LocalDynamoDbSetup _localDynamoDbSetup;
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        this._localDynamoDbSetup = new LocalDynamoDbSetup();
        await this._localDynamoDbSetup.SetupDynamoDb();
        await this._localDynamoDbSetup.CreateTables(DynamoDbConstants.PlayerTableName, null, null);
        this._dynamoDb = this._localDynamoDbSetup.GetClient();
        this._playerCloudService = new PlayerDynamoDbCloudService(this._dynamoDb);
        this._donationCloudService = new DonationDynamoDbCloudService(this._playerCloudService);
    }

    [Test]
    public async Task GetDonation_SuccessfullyGetsPlayerDonations()
    {
        var playerId = Guid.NewGuid().ToString();
        var donationId = Guid.NewGuid().ToString();
        await this.CreateDonationForPlayer(playerId, donationId);
        var donation = await this._donationCloudService.GetDonation(playerId, donationId);
        Assert.AreEqual(donationId, donation.Id);
    }
    
    [Test]
    public void GetDonation_ThrowsException_IfPlayerNotFound()
    {
         Assert.ThrowsAsync<ResourceNotFoundException>(() => this._donationCloudService.GetDonation("test", ""));
    }
    
    [Test]
    public async Task GetDonation_ThrowsException_IfDonationNotFound()
    {
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Dan"
        };
        await this._playerCloudService.CreatePlayer(player);
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._donationCloudService.GetDonation(player.Id, ""));
    }
    
    [Test]
    public async Task GetDonations_SuccessfullyGetsAllPlayerDonations()
    {
        var donation = new Donation(Guid.NewGuid().ToString(), 1, DateTime.Now, 1, "Test");
        var secondDonation = new Donation(Guid.NewGuid().ToString(), 2, DateTime.Now, 2, "Test");
        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Donations = new List<Donation>
            {
                donation, secondDonation
            }
        };
        await this._playerCloudService.CreatePlayer(player);
        var donations = await this._donationCloudService.GetDonations(player.Id);
        Assert.AreEqual(2, donations.Count);
    }
    
    [Test]
    public void GetDonations_ThrowsException_IfPlayerNotFound()
    {
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._donationCloudService.GetDonations("test"));
    }
    

    [Test]
    public async Task CreateDonation_SuccessfullyAddsToPlayer()
    {
        var playerId = Guid.NewGuid().ToString();
        var donationId = Guid.NewGuid().ToString();
        await this.CreateDonationForPlayer(playerId, donationId);
        var player = await this._playerCloudService.GetPlayerById(playerId);
        var donation = await this._donationCloudService.GetDonation(playerId, donationId);
        
        Assert.AreEqual(1, player.Donations.Count);

        var savedDonation = player.Donations.First();
        Assert.AreEqual(donation.Id, savedDonation.Id);
    }
    
    [Test]
    public async Task UpdateDonation_SuccessfullyUpdatesDonation()
    {
        var playerId = Guid.NewGuid().ToString();
        var donationId = Guid.NewGuid().ToString();
        await this.CreateDonationForPlayer(playerId, donationId);
        var player = await this._playerCloudService.GetPlayerById(playerId);
        var donation = player.Donations.First();
        donation.Amount = 2;

        await this._donationCloudService.UpdateDonation(player.Id, donation);
        player = await this._playerCloudService.GetPlayerById(player.Id);
        donation = player.Donations.First();

        Assert.AreEqual(2, donation.Amount);
    }
    
    [Test]
    public async Task DeleteDonation_SuccessfullyDeletesDonationFromPlayer()
    {
        var playerId = Guid.NewGuid().ToString();
        var donationId = Guid.NewGuid().ToString();
        await this.CreateDonationForPlayer(playerId, donationId);
        await this._donationCloudService.DeleteDonation(playerId, donationId);
        Assert.ThrowsAsync<ResourceNotFoundException>(() => this._donationCloudService.GetDonation(playerId, donationId));
    }

    private async Task CreateDonationForPlayer(string playerId, string donationId)
    {
        var player = new Player
        {
            Id = playerId,
            Name = "Dan"
        };
        var donation = new Donation
        {
            Id = donationId,
            CreatedDate = DateTime.Now,
            Amount = 1,
            CharityId = 1,
            CharityName = "Test"
        };
        await this._playerCloudService.CreatePlayer(player);
        await this._donationCloudService.Create(player.Id, donation);
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        this._localDynamoDbSetup.KillProcess();
    }
}