using JobMonitor.Application.ResponseModels;

namespace JobMonitor.Application.Interfaces;

public interface IHeadHunterService
{
    Task<List<VacancyResponse>> GetVacanciesAsync(string query, int page = 0, int perPage = 10);
}
