using Common.Models;

namespace Core.Services;

public interface ILockService
{
    Task<Lock> Create(Lock newLock);
    Task<List<Lock>> GetLocks();
    Task<List<Lock>> GetLocksForPlayers(List<string> playerIds);
    Task<Lock> GetLock(string id);
    Task DeleteLock(string id);
    Task<Lock> UpdateLock(Lock theLock);
}