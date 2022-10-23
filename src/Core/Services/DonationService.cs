using Cloud.Services;
using Common.Models;

namespace Core.Services;

public class DonationService : IDonationService
{
    private readonly IDonationCloudService _donationCloudService;

    public DonationService(IDonationCloudService donationCloudService)
    {
        this._donationCloudService = donationCloudService;
    }

    public async Task<Donation> Create(string playerId, Donation donation)
    {
        return await this._donationCloudService.Create(playerId, donation);
    }

    public async Task<List<Donation>> GetDonations(string playerId)
    {
        return await this._donationCloudService.GetDonations(playerId);
    }

    public async Task<Donation> GetDonation(string playerId, string id)
    {
        return await this._donationCloudService.GetDonation(playerId, id);
    }

    public async Task DeleteDonation(string playerId, string id)
    {
        await this._donationCloudService.DeleteDonation(playerId, id);
    }

    public async Task<Donation> UpdateDonation(string playerId, Donation donation)
    {
        return await this._donationCloudService.UpdateDonation(playerId, donation);
    }
}