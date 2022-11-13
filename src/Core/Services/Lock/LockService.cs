using Cloud.Services;

namespace Core.Services.Lock;

public class LockService : ILockService
{
    private readonly ILockCloudService _lockCloudService;
    
    public LockService(ILockCloudService lockCloudService)
    {
        this._lockCloudService = lockCloudService;
    }

    public async Task<Common.Models.Lock> Create(Common.Models.Lock newLock)
    {
        return await this._lockCloudService.Create(newLock);
    }

    public async Task<List<Common.Models.Lock>> GetAll()
    {
        return await this._lockCloudService.GetLocks();
    }

    public async Task<List<Common.Models.Lock>> GetLocksForPlayers(List<string> playerIds)
    {
        return await this._lockCloudService.GetLocksForPlayers(playerIds);
    }

    public async Task<Common.Models.Lock> GetById(string id)
    {
        return await this._lockCloudService.GetLock(id);
    }

    public async Task Delete(string id)
    {
        await this._lockCloudService.DeleteLock(id);
    }

    public async Task<Common.Models.Lock> Update(Common.Models.Lock theLock)
    {
        return await this._lockCloudService.UpdateLock(theLock);
    }
}