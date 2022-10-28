using Amazon.DynamoDBv2.Model;
using Cloud.Services;
using Common.Models;

namespace Core.Services;

public class PlayerService : IPlayerService
{

    private readonly IPlayerCloudService _playerCloudService;

    public PlayerService(IPlayerCloudService playerCloudService)
    {
        this._playerCloudService = playerCloudService;
    }

    public async Task<List<Player>> GetPlayers()
    {
        return await this._playerCloudService.GetPlayers();
    }

    public async Task<Player> GetPlayerById(string id)
    {
        return await this._playerCloudService.GetPlayerById(id);
    }

    public async Task<Player> CreatePlayer(Player player)
    {
        return await this._playerCloudService.CreatePlayer(player);
    }

    public async Task DeletePlayer(string id)
    {
        await this._playerCloudService.DeletePlayer(id);
    }

    public async Task<Player> UpdatePlayer(Player player)
    {
        return await this._playerCloudService.UpdatePlayer(player);
    }
}