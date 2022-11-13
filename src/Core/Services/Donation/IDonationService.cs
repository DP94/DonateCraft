namespace Core.Services.Donation;

public interface IDonationService
{
    Task<Common.Models.Donation> Create(string playerId, Common.Models.Donation donation);
    Task<List<Common.Models.Donation>> GetDonations(string playerId);
    Task<Common.Models.Donation> GetDonation(string playerId, string id);
    Task DeleteDonation(string playerId, string id);
    Task<Common.Models.Donation> UpdateDonation(string playerId, Common.Models.Donation donation);
}