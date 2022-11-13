using Cloud.Services;

namespace Core.Services.Player;

public class PlayerService : IPlayerService
{

    private readonly IPlayerCloudService _playerCloudService;

    public PlayerService(IPlayerCloudService playerCloudService)
    {
        this._playerCloudService = playerCloudService;
    }

    public async Task<List<Common.Models.Player>> GetAll()
    {
        return await this._playerCloudService.GetPlayers();
    }

    public async Task<Common.Models.Player> GetById(string id)
    {
        return await this._playerCloudService.GetPlayerById(id);
    }

    public async Task<Common.Models.Player> Create(Common.Models.Player player)
    {
        return await this._playerCloudService.CreatePlayer(player);
    }

    public async Task Delete(string id)
    {
        await this._playerCloudService.DeletePlayer(id);
    }

    public async Task<Common.Models.Player> Update(Common.Models.Player player)
    {
        return await this._playerCloudService.UpdatePlayer(player);
    }
}