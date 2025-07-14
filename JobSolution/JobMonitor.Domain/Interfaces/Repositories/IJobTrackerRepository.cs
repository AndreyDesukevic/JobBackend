namespace JobMonitor.Domain.Interfaces.Repositories;

public interface IJobTrackerRepository
{
    Task SaveJobAsync(string searchId, string jobId);
    Task<string?> GetJobIdAsync(string searchId);
    Task DeleteJobAsync(string searchId);
    Task<bool> IsJobCancelledAsync(string searchId);
}
