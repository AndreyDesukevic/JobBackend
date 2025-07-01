using JobMonitor.Domain.Interfaces.RabbitMq;

namespace JobMonitor.Application.RabbitMq;

public class SearchQueueService : ISearchQueueService
{
    private readonly RabbitMqService _rabbitMq;

    public SearchQueueService(RabbitMqService rabbitMq)
    {
        _rabbitMq = rabbitMq;
    }

    public async Task EnqueueSearchAsync(string userId, string searchId)
    {
        var message = new { UserId = userId, SearchId = searchId };
        await _rabbitMq.Publish("search-tasks", message);
    }
}
