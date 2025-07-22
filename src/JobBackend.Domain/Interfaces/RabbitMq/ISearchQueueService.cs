namespace JobBackend.Domain.Interfaces.RabbitMq;

public interface ISearchQueueService
{
    Task EnqueueSearchAsync(string userId, string searchId);
}
