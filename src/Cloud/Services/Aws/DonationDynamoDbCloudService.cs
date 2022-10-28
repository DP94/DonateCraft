using Common.Exceptions;
using Common.Models;

namespace Cloud.Services.Aws;

public class DonationDynamoDbCloudService : IDonationCloudService
{
    private readonly IPlayerCloudService _playerCloudService;

    public DonationDynamoDbCloudService(IPlayerCloudService playerCloudService)
    {
        this._playerCloudService = playerCloudService;
    }

    public async Task<Donation> Create(string playerId, Donation donation)
    {
        var player = await this._playerCloudService.GetPlayerById(playerId);
        player.Donations.Add(donation);
        await this._playerCloudService.UpdatePlayer(player);
        return donation;
    }

    public async Task<List<Donation>> GetDonations(string playerId)
    {
        var result = await this._playerCloudService.GetPlayerById(playerId);
        return result.Donations;
    }

    public async Task<Donation> GetDonation(string playerId, string id)
    {
        var player = await this._playerCloudService.GetPlayerById(playerId);
        foreach (var donation in player.Donations.Where(d => d.Id == id))
        {
            return donation;
        }
        throw new ResourceNotFoundException($"Donation with {id} not found");
    }

    public async Task DeleteDonation(string playerId, string id)
    {
        var player = await this._playerCloudService.GetPlayerById(playerId);
        var donationToDelete = player.Donations.Find(d => d.Id == id);
        if (donationToDelete == null)
        {
            throw new ResourceNotFoundException($"Donation {id} not found for player {playerId}");
        }
        player.Donations.Remove(donationToDelete);
        await this._playerCloudService.UpdatePlayer(player);
    }

    public async Task<Donation> UpdateDonation(string playerId, Donation donation)
    {

        var player = await this._playerCloudService.GetPlayerById(playerId);
        var donationToUpdate = player.Donations.Find(d => d.Id == donation.Id);
        if (donationToUpdate == null)
        {
            throw new ResourceNotFoundException($"Donation {donation.Id} not found for player {playerId}");
        }
        //Only reason is mutable
        donationToUpdate.Amount = donation.Amount;
        donationToUpdate.CharityId = donation.CharityId;
        donationToUpdate.CharityName = donation.CharityName;
        await this._playerCloudService.UpdatePlayer(player);
        return donationToUpdate;
    }
}