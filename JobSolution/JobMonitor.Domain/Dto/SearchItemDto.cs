using System.Text.Json.Serialization;

namespace JobMonitor.Domain.Dto;

public class SearchItemDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
