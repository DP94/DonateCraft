namespace Core.Services;

public interface WithIdService<T> : BaseService
{
    Task<T> GetById(string id);
    Task<List<T>> GetAll();
    Task Delete(string id);
    Task<T> Create(T t);
    Task<T> Update(T t);
}