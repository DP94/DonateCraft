using Common.Models;

namespace Cloud.Services;

public interface IRevivalQueueService
{
    Task Enqueue(RevivalMessage message);
}