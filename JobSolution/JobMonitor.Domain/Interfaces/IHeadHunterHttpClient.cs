using JobMonitor.Domain.ResponseModels;
using System.Text.Json;

namespace JobMonitor.Domain.Interfaces;

public interface IHeadHunterHttpClient
{
    Task<JsonDocument> GetVacanciesJsonAsync(string query, int page, int perPage);
    Task<string> CreateSavedSearchAsync(string hhAccessToken, string name, string text);
    Task<List<SavedSearchResponse>> GetSavedSearchesAsync(string hhAccessToken, int page, int perPage, string locale, string host);
    Task<string> GetSavedSearchByIdAsync(string hhAccessToken, string id, string locale, string host);
    Task DeleteSavedSearchAsync(string hhAccessToken, string id, string local, string host);
}
