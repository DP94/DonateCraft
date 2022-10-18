using Common.Models;

namespace Core.Services;

public interface IDeathService
{
    Task<List<Death>> GetDeaths();
    Task<Death> GetDeathById(string id);

    Task<Death> CreateDeath(Death death);

    Task DeleteDeath(string id);

    Task<Death> UpdateDeath(Death death);
}