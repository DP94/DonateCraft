using Cloud.Services;
using Common.Models;

namespace Core.Services;

public class LockService : ILockService
{
    private readonly ILockCloudService _lockCloudService;
    
    public LockService(ILockCloudService lockCloudService)
    {
        this._lockCloudService = lockCloudService;
    }

    public async Task<Lock> Create(Lock newLock)
    {
        return await this._lockCloudService.Create(newLock);
    }

    public async Task<List<Lock>> GetLocks()
    {
        return await this._lockCloudService.GetLocks();
    }

    public async Task<Lock> GetLock(string id)
    {
        return await this._lockCloudService.GetLock(id);
    }

    public async Task DeleteLock(string id)
    {
        await this._lockCloudService.DeleteLock(id);
    }
}