namespace Core.Services.Lock;

public interface ILockService : WithIdService<Common.Models.Lock>
{
    Task<List<Common.Models.Lock>> GetLocksForPlayers(List<string> playerIds);
}