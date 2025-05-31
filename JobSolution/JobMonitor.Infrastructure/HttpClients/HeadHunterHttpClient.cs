using JobMonitor.Domain.Interfaces;
using JobMonitor.Domain.ResponseModels;
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

    public async Task<string> CreateSavedSearchAsync(string hhAccessToken, string name, string text)
    {
        var url = $"{BaseUrl}/saved_searches/vacancies";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);

        var parameters = new List<KeyValuePair<string, string>>
    {
        new("name", name),
        new("text", text)
        // Добавь остальные параметры по мере необходимости
    };
        request.Content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        return content;
    }

    public async Task<List<SavedSearchResponse>> GetSavedSearchesAsync(string hhAccessToken, int page = 0, int perPage = 10, string locale = "RU", string host = "hh.ru")
    {
        var url = $"{BaseUrl}/saved_searches/vacancies?page={page}&per_page={perPage}&locale={locale}&host={host}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);
        request.Headers.Add("User-Agent", "JobMonitorApp/1.0 (my-app-feedback@example.com)");

        var response = await _httpClient.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var result = new List<SavedSearchResponse>();
        if (root.TryGetProperty("items", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                result.Add(new SavedSearchResponse
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    CreatedAt = item.GetProperty("created_at").GetDateTime(),
                    ItemsCount = item.GetProperty("items").GetProperty("count").GetInt32(),
                    NewItemsCount = item.GetProperty("new_items").GetProperty("count").GetInt32()
                });
            }
        }
        return result;
    }

    public async Task<string> GetSavedSearchByIdAsync(string hhAccessToken, string id, string locale = "RU", string host = "hh.ru")
    {
        var url = $"{BaseUrl}/saved_searches/vacancies/{id}?locale={locale}&host={host}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        return content;
    }

    public async Task DeleteSavedSearchAsync(string hhAccessToken, string id, string locale = "RU", string host = "hh.ru")
    {
        var url = $"{BaseUrl}/saved_searches/vacancies/{id}?locale={locale}&host={host}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);
        request.Headers.Add("User-Agent", "JobMonitorApp/1.0 (my-app-feedback@example.com)");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"HeadHunter API error: {await response.Content.ReadAsStringAsync()}");
    }
}
