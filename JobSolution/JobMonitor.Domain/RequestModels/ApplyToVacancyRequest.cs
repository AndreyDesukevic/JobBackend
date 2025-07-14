using System.Text.Json.Serialization;

namespace JobMonitor.Domain.RequestModels;

public class ApplyToVacancyRequest
{
    [JsonPropertyName("resume_id")]
    public string ResumeId { get; set; } = null!;

    [JsonPropertyName("vacancy_id")]
    public string VacancyId { get; set; } = null!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}
