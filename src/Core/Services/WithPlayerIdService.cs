namespace Core.Services;

public interface WithPlayerIdService<T> : BaseService
{
    Task<T> GetByPlayerId(string playerId, string id);
    Task<List<T>> GetAllForPlayerId(string playerId);
    Task Delete(string playerId, string id);
    Task<T> Create(string playerId, T t);
    Task<T> Update(string playerId, T t);
}