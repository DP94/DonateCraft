using Amazon.DynamoDBv2.Model;
using Cloud.Services;
using Common.Models;

namespace Core.Services;

public class PlayerService : IPlayerService
{

    private readonly IPlayerDynamoDbStorageService _playerDynamoDbStorageService;

    public PlayerService(IPlayerDynamoDbStorageService playerDynamoDbStorageService)
    {
        _playerDynamoDbStorageService = playerDynamoDbStorageService;
    }

    public async Task<List<Player>> GetPlayers()
    {
        return await this._playerDynamoDbStorageService.GetPlayers();
    }

    public async Task<Player?> GetPlayerById(string id)
    {
        try
        {
            return await this._playerDynamoDbStorageService.GetPlayerById(id);
        }
        catch (ResourceNotFoundException)
        {
            return null;
        }
    }

    public async Task<Player> CreatePlayer(Player player)
    {
        return await this._playerDynamoDbStorageService.CreatePlayer(player);
    }

    public async Task DeletePlayer(string id)
    {
        await this._playerDynamoDbStorageService.DeletePlayer(id);
    }

    public Task<Player> UpdatePlayer(Player player)
    {
        throw new NotImplementedException();
    }
}