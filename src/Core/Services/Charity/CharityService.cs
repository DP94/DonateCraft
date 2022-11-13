using Cloud.Services;

namespace Core.Services.Charity;

public class CharityService : ICharityService
{
    private readonly ICharityCloudService _charityCloudService;

    public CharityService(ICharityCloudService charityCloudService)
    {
        this._charityCloudService = charityCloudService;
    }

    public async Task<Common.Models.Charity> GetById(string id)
    {
        return await this._charityCloudService.GetCharityById(id);
    }

    public async Task<List<Common.Models.Charity>> GetAll()
    {
        return await this._charityCloudService.GetCharities();
    }

    public async Task Delete(string id)
    {
        await this._charityCloudService.DeleteCharity(id);
    }

    public async Task<Common.Models.Charity> Create(Common.Models.Charity charity)
    {
        return await this._charityCloudService.CreateCharity(charity);
    }

    public async Task<Common.Models.Charity> Update(Common.Models.Charity charity)
    {
        return await this._charityCloudService.UpdateCharity(charity);
    }
}