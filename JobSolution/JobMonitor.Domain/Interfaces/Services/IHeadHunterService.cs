using JobMonitor.Domain.ResponseModels;

namespace JobMonitor.Domain.Interfaces.Services;

public interface IHeadHunterService
{
    Task<List<VacancyResponse>> GetVacanciesAsync(string query, int page = 0, int perPage = 10);
}
