using Cloud.Services;

namespace Core.Services.Death;

public class DeathService : IDeathService
{

    private IDeathCloudService _deathCloudService;

    public DeathService(IDeathCloudService deathCloudService)
    {
        this._deathCloudService = deathCloudService;
    }

    public async Task<List<Common.Models.Death>> GetDeaths(string playerId)
    {
        return await this._deathCloudService.GetDeaths(playerId);
    }

    public async Task<Common.Models.Death> GetDeathById(string playerId, string id)
    {
        return await this._deathCloudService.GetDeathById(playerId, id);;
    }

    public async Task<Common.Models.Death> CreateDeath(string playerId, Common.Models.Death death)
    {
        return await this._deathCloudService.CreateDeath(playerId, death);
    }

    public async Task DeleteDeath(string playerId, string id)
    {
        await this._deathCloudService.DeleteDeath(playerId, id);
    }

    public async Task<Common.Models.Death> UpdateDeath(string playerId, Common.Models.Death death)
    {
        return await this._deathCloudService.UpdateDeath(playerId, death);
    }
}