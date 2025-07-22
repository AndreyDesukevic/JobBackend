using System.Text.Json.Serialization;

namespace JobBackend.Domain.Dto;

public class VacancyShortDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("employer")]
    public EmployerDto Employer { get; set; } = new ();
}