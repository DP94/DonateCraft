using Common.Models;

namespace Core.Services;

public interface ICharityService
{
    Task<Charity> GetCharityById(string id);
    Task<List<Charity>> GetCharities();
    Task DeleteCharity(string id);
    Task<Charity> CreateCharity(Charity charity);
    Task<Charity> UpdateCharity(Charity charity);
}