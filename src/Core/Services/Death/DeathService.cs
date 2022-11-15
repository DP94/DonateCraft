using Cloud.Services;

namespace Core.Services.Death;

public class DeathService : IDeathService
{

    private readonly IDeathCloudService _deathCloudService;

    public DeathService(IDeathCloudService deathCloudService)
    {
        this._deathCloudService = deathCloudService;
    }

    public async Task<List<Common.Models.Death>> GetAllForPlayerId(string playerId)
    {
        return await this._deathCloudService.GetDeaths(playerId);
    }

    public async Task<Common.Models.Death> GetByPlayerId(string playerId, string id)
    {
        return await this._deathCloudService.GetDeathById(playerId, id);;
    }

    public async Task<Common.Models.Death> Create(string playerId, Common.Models.Death death)
    {
        return await this._deathCloudService.CreateDeath(playerId, death);
    }

    public async Task Delete(string playerId, string id)
    {
        await this._deathCloudService.DeleteDeath(playerId, id);
    }

    public async Task<Common.Models.Death> Update(string playerId, Common.Models.Death death)
    {
        return await this._deathCloudService.UpdateDeath(playerId, death);
    }
}