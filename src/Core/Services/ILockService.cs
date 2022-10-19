using Common.Models;

namespace Core.Services;

public interface ILockService
{
    Task<Lock> Create(Lock newLock);
    Task<List<Lock>> GetLocks();
    Task<Lock> GetLock(string id);
    Task DeleteLock(string id);
}