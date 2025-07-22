using JobBackend.Application.Services;
using JobBackend.Domain.Dto;
using JobBackend.Domain.Interfaces;
using JobBackend.Domain.Interfaces.Services;
using JobBackend.Domain.RequestModels;
using JobBackend.Domain.ResponseModels;
using System.Text.Json;

namespace JobBackend.Infrastructure.Services;

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

    public async Task<string> CreateSavedSearchAsync(string hhAccessToken, string name, string text)
    {
        return await _httpClient.CreateSavedSearchAsync(hhAccessToken, name, text);
    }

    public async Task<List<SavedSearchResponse>> GetSavedSearchesAsync(string hhAccessToken, int page, int perPage, string locale, string host)
    {
        return await _httpClient.GetSavedSearchesAsync(hhAccessToken, page, perPage, locale, host);
    }
    public async Task<string> GetSavedSearchByIdAsync(string? hhAccessToken, string id, string locale, string host)
    {
        return await _httpClient.GetSavedSearchByIdAsync(hhAccessToken, id, locale, host);
    }
    public async Task DeleteSavedSearchAsync(string hhAccessToken, string id, string locale, string host)
    {
        await _httpClient.DeleteSavedSearchAsync(hhAccessToken, id, locale, host);
    }

    public async Task<List<VacancyShortDto>> GetAllVacanciesAsync(string baseUrl)
    {
        var allVacancies = new List<VacancyShortDto>();
        int page = 0;
        int perPage = 100;
        int totalPages = 1;

        do
        {
            var pagedUrl = $"{baseUrl}&per_page={perPage}&page={page}";
            var json = await _httpClient.GetVacanciesByUrlAsync(pagedUrl);

            var vacancies = JsonSerializer.Deserialize<VacanciesDto>(json);

            if (vacancies?.Items != null)
                allVacancies.AddRange(vacancies.Items);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("pages", out var pagesProp))
                totalPages = pagesProp.GetInt32();

            page++;
        }
        while (page < totalPages);

        return allVacancies;
    }

    public async Task<JsonDocument> GetVacancyByIdAsync(string id, string hhAccessToken)
    {
        return await _httpClient.GetVacancyByIdAsync(id, hhAccessToken);
    }

    public async Task<string> GetVacancyDescriptionByIdAsync(string id, string hhAccessToken)
    {
        var vacancyJson = await _httpClient.GetVacancyByIdAsync(id, hhAccessToken);
        var description = vacancyJson.RootElement.GetProperty("description").GetString();

        return description ?? string.Empty;
    }

    public async Task<ResumeAggregatedInfoDto?> GetResumeTextForAiAsync(string resumeId, string accessToken)
    {
        var doc = await _httpClient.GetResumeByIdAsync(resumeId, accessToken);
        var root = doc.RootElement;

        var descriptions = new List<string>();
        var allSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? summary = null;

        if (root.TryGetProperty("skills", out var summaryProp) && summaryProp.ValueKind == JsonValueKind.String)
        {
            summary = summaryProp.GetString()?.Trim();
        }

        if (root.TryGetProperty("experience", out var experience) && experience.ValueKind == JsonValueKind.Array)
        {
            foreach (var job in experience.EnumerateArray())
            {
                if (job.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
                {
                    var text = desc.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        descriptions.Add(text);
                }
            }
        }

        if (root.TryGetProperty("skill_set", out var skillSet) && skillSet.ValueKind == JsonValueKind.Array)
        {
            foreach (var skill in skillSet.EnumerateArray())
            {
                if (skill.ValueKind == JsonValueKind.String)
                {
                    var skillName = skill.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(skillName))
                        allSkills.Add(skillName);
                }
            }
        }

        return new ResumeAggregatedInfoDto
        {
            Summary = summary ?? "",
            CombinedExperienceDescription = string.Join("\n\n", descriptions),
            CombinedSkills = string.Join(", ", allSkills.OrderBy(s => s))
        };
    }

    public async Task ApplyWithGeneratedLetterAsync(string accessToken, string vacancyId, string resumeId, string letter)
    {

        var request = new ApplyToVacancyRequest
        {
            ResumeId = resumeId,
            VacancyId = vacancyId,
            Message = letter
        };

        await _httpClient.ApplyToVacancyAsync(accessToken, request);
    }

}
