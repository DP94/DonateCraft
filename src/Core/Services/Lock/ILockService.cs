namespace Core.Services.Lock;

public interface ILockService
{
    Task<Common.Models.Lock> Create(Common.Models.Lock newLock);
    Task<List<Common.Models.Lock>> GetLocks();
    Task<List<Common.Models.Lock>> GetLocksForPlayers(List<string> playerIds);
    Task<Common.Models.Lock> GetLock(string id);
    Task DeleteLock(string id);
    Task<Common.Models.Lock> UpdateLock(Common.Models.Lock theLock);
}