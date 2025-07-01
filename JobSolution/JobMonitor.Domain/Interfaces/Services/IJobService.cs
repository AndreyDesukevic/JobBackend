namespace JobMonitor.Domain.Interfaces.Services;

public interface IJobService
{
    Task RunSearch(string searchId, string userId, CancellationToken cancellationToken);
    Task StopSearch(string searchId, string userId);
}
