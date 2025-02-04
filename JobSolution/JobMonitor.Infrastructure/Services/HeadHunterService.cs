using JobMonitor.Application.Interfaces;
using JobMonitor.Application.ResponseModels;
using JobMonitor.Infrastructure.HttpClients;
using System.Text.Json;

namespace JobMonitor.Infrastructure.Services;

public class HeadHunterService : IHeadHunterService
{
    private readonly IHeadHunterHttpClient _httpClient;

    public HeadHunterService(IHeadHunterHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<VacancyResponse>> GetVacanciesAsync(string query, int page = 0, int perPage = 10)
    {
        using JsonDocument doc = await _httpClient.GetVacanciesJsonAsync(query, page, perPage);
        var items = doc.RootElement.GetProperty("items");

        var vacancies = new List<VacancyResponse>();
        foreach (var item in items.EnumerateArray())
        {
            vacancies.Add(new VacancyResponse
            {
                Id = item.GetProperty("id").GetInt32(),
                Title = item.GetProperty("name").GetString(),
                City = item.GetProperty("area").GetProperty("name").GetString(),
                SalaryFrom = item.TryGetProperty("salary", out var salary) && salary.TryGetProperty("from", out var from) ? from.GetInt32() : (int?)null,
                SalaryTo = item.TryGetProperty("salary", out salary) && salary.TryGetProperty("to", out var to) ? to.GetInt32() : (int?)null,
                Currency = item.TryGetProperty("salary", out salary) && salary.TryGetProperty("currency", out var currency) ? currency.GetString() : null,
                Company = item.GetProperty("employer").GetProperty("name").GetString(),
                Url = item.GetProperty("alternate_url").GetString()
            });
        }

        return vacancies;
    }
}
