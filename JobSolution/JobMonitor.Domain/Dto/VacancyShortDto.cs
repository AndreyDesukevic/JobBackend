using System.Text.Json.Serialization;

namespace JobMonitor.Domain.Dto;

public class VacancyShortDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}