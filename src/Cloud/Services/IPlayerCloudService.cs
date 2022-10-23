using Common.Models;

namespace Cloud.Services;

public interface IPlayerCloudService
{
    Task<List<Player>> GetPlayers();
    Task<Player> GetPlayerById(string id);

    Task<Player> CreatePlayer(Player player);

    Task DeletePlayer(string id);

    Task<Player> UpdatePlayer(Player player);
}