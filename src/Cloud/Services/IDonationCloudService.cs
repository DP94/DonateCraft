using Common.Models;

namespace Cloud.Services;

public interface IDonationCloudService
{
    Task<Donation> Create(string playerId, Donation donation);
    Task<List<Donation>> GetDonations(string playerId);
    Task<Donation> GetDonation(string playerId, string id);
    Task DeleteDonation(string playerId, string id);
    Task<Donation> UpdateDonation(string playerId, Donation donation);
}