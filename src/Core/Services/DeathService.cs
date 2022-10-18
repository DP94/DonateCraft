using Cloud.Services;
using Common.Models;

namespace Core.Services;

public class DeathService : IDeathService
{

    private IDeathCloudService _deathCloudService;

    public DeathService(IDeathCloudService deathCloudService)
    {
        this._deathCloudService = deathCloudService;
    }

    public async Task<List<Death>> GetDeaths()
    {
        return await this._deathCloudService.GetDeaths();
    }

    public async Task<Death> GetDeathById(string id)
    {
        return await this._deathCloudService.GetDeathById(id);;
    }

    public async Task<Death> CreateDeath(Death death)
    {
        return await this._deathCloudService.CreateDeath(death);
    }

    public async Task DeleteDeath(string id)
    {
        await this._deathCloudService.DeleteDeath(id);
    }

    public async Task<Death> UpdateDeath(Death death)
    {
        return await this._deathCloudService.UpdateDeath(death);
    }
}