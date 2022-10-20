using Cloud.Services;
using Common.Models;

namespace Core.Services;

public class CharityService : ICharityService
{
    private ICharityCloudService _charityCloudService;

    public CharityService(ICharityCloudService charityCloudService)
    {
        this._charityCloudService = charityCloudService;
    }

    public async Task<Charity> GetCharityById(string id)
    {
        return await this._charityCloudService.GetCharityById(id);
    }

    public async Task<List<Charity>> GetCharities()
    {
        return await this._charityCloudService.GetCharities();
    }

    public async Task DeleteCharity(string id)
    {
        await this._charityCloudService.DeleteCharity(id);
    }

    public async Task<Charity> CreateCharity(Charity charity)
    {
        return await this._charityCloudService.CreateCharity(charity);
    }

    public async Task<Charity> UpdateCharity(Charity charity)
    {
        return await this._charityCloudService.UpdateCharity(charity);
    }
}