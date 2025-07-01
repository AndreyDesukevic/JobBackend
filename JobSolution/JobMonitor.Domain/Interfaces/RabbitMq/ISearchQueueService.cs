namespace JobMonitor.Domain.Interfaces.RabbitMq;

public interface ISearchQueueService
{
    Task EnqueueSearchAsync(string userId, string searchId);
}
