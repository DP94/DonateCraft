using Common.Models;

namespace Cloud.Services;

public interface ILockCloudService
{
    Task<Lock> Create(Lock newLock);
    Task<List<Lock>> GetLocks();
    Task<List<Lock>> GetLocksForPlayers(List<string> playerIds);
    Task<Lock> GetLock(string id);
    Task DeleteLock(string id);
    Task<Lock> UpdateLock(Lock theLock);
}