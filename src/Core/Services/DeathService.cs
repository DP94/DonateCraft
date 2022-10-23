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

    public async Task<List<Death>> GetDeaths(string playerId)
    {
        return await this._deathCloudService.GetDeaths(playerId);
    }

    public async Task<Death> GetDeathById(string playerId, string id)
    {
        return await this._deathCloudService.GetDeathById(playerId, id);;
    }

    public async Task<Death> CreateDeath(string playerId, Death death)
    {
        return await this._deathCloudService.CreateDeath(playerId, death);
    }

    public async Task DeleteDeath(string playerId, string id)
    {
        await this._deathCloudService.DeleteDeath(playerId, id);
    }

    public async Task<Death> UpdateDeath(string playerId, Death death)
    {
        return await this._deathCloudService.UpdateDeath(playerId, death);
    }
}