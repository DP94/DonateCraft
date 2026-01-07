using Common.Models;

namespace RevivalLambda.Services;

public interface IRevivalService
{
    Task ProcessRevival(RevivalMessage message);
}