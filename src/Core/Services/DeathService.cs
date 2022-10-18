using Cloud.Services;
using Common.Models;

namespace Core.Services;

public class DeathService : IDeathService
{

    private IDeathDynamoDbStorageService _deathDynamoDbStorageService;

    public DeathService(IDeathDynamoDbStorageService deathDynamoDbStorageService)
    {
        this._deathDynamoDbStorageService = deathDynamoDbStorageService;
    }

    public async Task<List<Death>> GetDeaths()
    {
        return await this._deathDynamoDbStorageService.GetDeaths();
    }

    public async Task<Death> GetDeathById(string id)
    {
        return await this._deathDynamoDbStorageService.GetDeathById(id);;
    }

    public async Task<Death> CreateDeath(Death death)
    {
        return await this._deathDynamoDbStorageService.CreateDeath(death);
    }

    public async Task DeleteDeath(string id)
    {
        await this._deathDynamoDbStorageService.DeleteDeath(id);
    }

    public async Task<Death> UpdateDeath(Death death)
    {
        return await this._deathDynamoDbStorageService.UpdateDeath(death);
    }
}