using JobBackend.Domain.Dto;
using JobBackend.Domain.RequestModels;
using JobBackend.Domain.ResponseModels;
using System.Text.Json;

namespace JobBackend.Domain.Interfaces.Services;

public interface IHeadHunterService
{
    Task<List<VacancyResponse>> GetVacanciesAsync(string query, int page = 0, int perPage = 10);
    Task<string> CreateSavedSearchAsync(string hhAccessToken, string name, string text);
    Task<List<SavedSearchResponse>> GetSavedSearchesAsync(string hhAccessToken, int page, int perPage, string locale, string host);
    Task<string> GetSavedSearchByIdAsync(string? hhAccessToken, string id, string locale, string host);
    Task DeleteSavedSearchAsync(string hhAccessToken, string id, string locale, string host);
    Task<List<VacancyShortDto>> GetAllVacanciesAsync(string baseUrl);
    Task<JsonDocument> GetVacancyByIdAsync(string id, string hhAccessToken);
    Task<string> GetVacancyDescriptionByIdAsync(string id, string hhAccessToken);
    Task<ResumeAggregatedInfoDto?> GetResumeTextForAiAsync(string resumeId, string accessToken);
    Task ApplyWithGeneratedLetterAsync(string accessToken, string vacancyId, string resumeId, string letter);
}
