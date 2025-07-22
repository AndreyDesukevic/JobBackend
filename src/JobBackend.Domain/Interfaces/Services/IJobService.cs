namespace JobBackend.Domain.Interfaces.Services;

public interface IJobService
{
    Task<string> RunSearchJobAsync(string searchId, string userId);
    Task<bool> CancelSearchJobAsync(string searchId);
    Task<string?> GetJobIdBySearchIdAsync(string searchId);
}
