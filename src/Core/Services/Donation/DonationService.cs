﻿using Cloud.Services;

namespace Core.Services.Donation;

public class DonationService : IDonationService
{
    private readonly IDonationCloudService _donationCloudService;

    public DonationService(IDonationCloudService donationCloudService)
    {
        this._donationCloudService = donationCloudService;
    }

    public async Task<Common.Models.Donation> Create(string playerId, Common.Models.Donation donation)
    {
        return await this._donationCloudService.Create(playerId, donation);
    }

    public async Task<List<Common.Models.Donation>> GetAllForPlayerId(string playerId)
    {
        return await this._donationCloudService.GetDonations(playerId);
    }

    public async Task<Common.Models.Donation> GetByPlayerId(string playerId, string id)
    {
        return await this._donationCloudService.GetDonation(playerId, id);
    }

    public async Task Delete(string playerId, string id)
    {
        await this._donationCloudService.DeleteDonation(playerId, id);
    }

    public async Task<Common.Models.Donation> Update(string playerId, Common.Models.Donation donation)
    {
        return await this._donationCloudService.UpdateDonation(playerId, donation);
    }
}