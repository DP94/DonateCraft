using Common.Models;

namespace Core.Services;

public interface IPlayerService
{
    Task<List<Player>> GetPlayers();
    Task<Player> GetPlayerById(string id);

    Task<Player> CreatePlayer(Player player);

    Task DeletePlayer(string id);

    Task<Player> UpdatePlayer(Player player);
}