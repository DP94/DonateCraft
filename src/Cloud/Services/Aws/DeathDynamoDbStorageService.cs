using Amazon.Lambda.Core;
using Common.Exceptions;
using Common.Models;

namespace Cloud.Services.Aws;

public class DeathDynamoDbStorageService : IDeathCloudService
{
    private readonly IPlayerCloudService _playerCloudService;
    private readonly ILockCloudService _lockCloudService;
    
    public DeathDynamoDbStorageService(IPlayerCloudService playerCloudService, ILockCloudService lockCloudService)
    {
        this._playerCloudService = playerCloudService;
        this._lockCloudService = lockCloudService;
    }

    public async Task<List<Death>> GetDeaths(string playerId)
    {
        var result = await this._playerCloudService.GetPlayerById(playerId);
        return result.Deaths;
    }

    public async Task<Death?> GetDeathById(string playerId, string id)
    {
        var player = await this._playerCloudService.GetPlayerById(playerId);
        foreach (var death in player.Deaths.Where(death => death.Id == id))
        {
            return death;
        }
        throw new ResourceNotFoundException($"Death with {id} not found");
    }

    public async Task<Death> CreateDeath(string playerId, Death death)
    {
        var player = await this._playerCloudService.GetPlayerById(playerId);
        LambdaLogger.Log($"Player: {player.ToString()}");
        player.Deaths.Add(death);
        await this._playerCloudService.UpdatePlayer(player);
        LambdaLogger.Log("Updated player");

        await this._lockCloudService.Create(new Lock
        {
            Id = playerId,
            Unlocked = false
        });
        LambdaLogger.Log("Created lock");
        
        return death;
    }

    public async Task DeleteDeath(string playerId, string id)
    {
        var player = await this._playerCloudService.GetPlayerById(playerId);
        var deathToDelete = player.Deaths.Find(death => death.Id == id);
        if (deathToDelete == null)
        {
            throw new ResourceNotFoundException($"Death {id} not found for player {playerId}");
        }
        player.Deaths.Remove(deathToDelete);
        await this._playerCloudService.UpdatePlayer(player);
    }

    public async Task<Death> UpdateDeath(string playerId, Death death)
    {

        var player = await this._playerCloudService.GetPlayerById(playerId);
        var deathToDelete = player.Deaths.Find(d => d.Id == death.Id);
        if (deathToDelete == null)
        {
            throw new ResourceNotFoundException($"Death {death.Id} not found for player {playerId}");
        }
        //Only reason is mutable
        deathToDelete.Reason = death.Reason;
        await this._playerCloudService.UpdatePlayer(player);
        return deathToDelete;
    }
    
}