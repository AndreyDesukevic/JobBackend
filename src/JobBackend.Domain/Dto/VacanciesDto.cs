using System.Text.Json.Serialization;

namespace JobBackend.Domain.Dto;

public class VacanciesDto
{
    [JsonPropertyName("items")]
    public List<VacancyShortDto> Items { get; set; } = new();
}
