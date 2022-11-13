namespace Core.Services.Death;

public interface IDeathService
{
    Task<List<Common.Models.Death>> GetDeaths(string playerId);
    Task<Common.Models.Death> GetDeathById(string playerId, string id);

    Task<Common.Models.Death> CreateDeath(string playerId, Common.Models.Death death);

    Task DeleteDeath(string playerId, string id);

    Task<Common.Models.Death> UpdateDeath(string playerId, Common.Models.Death death);
}