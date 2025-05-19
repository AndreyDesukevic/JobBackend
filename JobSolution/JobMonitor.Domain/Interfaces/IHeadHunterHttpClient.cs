using System.Text.Json;

namespace JobMonitor.Domain.Interfaces;

public interface IHeadHunterHttpClient
{
    Task<JsonDocument> GetVacanciesJsonAsync(string query, int page, int perPage);
}
