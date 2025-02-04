using JobMonitor.Application.Interfaces;
using System.Text.Json;

namespace JobMonitor.Infrastructure.HttpClients;

public class HeadHunterHttpClient : IHeadHunterHttpClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.hh.ru";

    public HeadHunterHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "JobMonitorApp/1.0");
    }

    public async Task<JsonDocument> GetVacanciesJsonAsync(string query, int page, int perPage)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be empty", nameof(query));
        }

        string encodedQuery = Uri.EscapeDataString(query);
        string url = $"{BaseUrl}/vacancies?text={encodedQuery}&search_field=name&page={page}&per_page={perPage}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(json);
    }
}
