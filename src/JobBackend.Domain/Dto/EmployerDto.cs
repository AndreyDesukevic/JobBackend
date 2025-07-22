using System.Text.Json.Serialization;

namespace JobBackend.Domain.Dto;

public class EmployerDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
