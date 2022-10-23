using Common.Models;

namespace Cloud.Services;

public interface IDeathCloudService
{
    Task<List<Death>> GetDeaths(string playerId);
    Task<Death?> GetDeathById(string playerId, string id);

    Task<Death> CreateDeath(string playerId, Death death);

    Task DeleteDeath(string playerId, string id);

    Task<Death> UpdateDeath(string playerId, Death death);
}