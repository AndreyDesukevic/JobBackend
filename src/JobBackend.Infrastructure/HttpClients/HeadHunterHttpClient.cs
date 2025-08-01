﻿using JobBackend.Domain.Interfaces;
using JobBackend.Domain.RequestModels;
using JobBackend.Domain.ResponseModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace JobBackend.Infrastructure.HttpClients;

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
        };
        request.Content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        return content;
    }

    public async Task<List<SavedSearchResponse>> GetSavedSearchesAsync(string hhAccessToken, int page = 0, int perPage = 10, string locale = "RU", string host = "hh.ru")
    {
        var url = $"{BaseUrl}/saved_searches/vacancies?page={page}&per_page=10&locale={locale}&host={host}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var result = new List<SavedSearchResponse>();
        if (root.TryGetProperty("items", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                var createdAt = item.TryGetProperty("created_at", out var createdAtProp) && createdAtProp.ValueKind == JsonValueKind.String
                    ? DateTime.Parse(createdAtProp.GetString())
                    : DateTime.MinValue;
                var itemsCount = item.TryGetProperty("items", out var itemsProp) && itemsProp.TryGetProperty("count", out var countProp)
                    ? countProp.GetInt32()
                    : 0;
                var newItemsCount = item.TryGetProperty("new_items", out var newItemsProp) && newItemsProp.TryGetProperty("count", out var newCountProp)
                    ? newCountProp.GetInt32()
                    : 0;

                result.Add(new SavedSearchResponse
                {
                    Id = id,
                    Name = name,
                    CreatedAt = createdAt,
                    ItemsCount = itemsCount,
                    NewItemsCount = newItemsCount
                });
            }
        }
        return result;
    }

    public async Task<string> GetSavedSearchByIdAsync(string? hhAccessToken, string id, string locale = "RU", string host = "hh.ru")
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

    public async Task<string> GetVacanciesByUrlAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<JsonDocument> GetVacancyByIdAsync(string id, string hhAccessToken)
    {
        var url = $"{BaseUrl}/vacancies/{id}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(json);
    }

    public async Task<JsonDocument> GetResumesByUserIdAsync(string id, string hhAccessToken)
    {
        var url = $"{BaseUrl}/vacancies/{id}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(json);
    }

    public async Task<JsonDocument> GetResumeByIdAsync(string resumeId, string hhAccessToken)
    {
        var url = $"{BaseUrl}/resumes/{resumeId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hhAccessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(json);
    }

    public async Task ApplyToVacancyAsync(string accessToken, ApplyToVacancyRequest requestDto)
    {
        var json = JsonConvert.SerializeObject(requestDto);
        var request = new HttpRequestMessage(HttpMethod.Post, $"vacancies/{requestDto.VacancyId}/feedback")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ошибка при отклике: {response.StatusCode}\n{error}");
        }
    }
}
