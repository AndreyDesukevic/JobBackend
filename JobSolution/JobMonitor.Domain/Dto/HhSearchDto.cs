using System.Text.Json.Serialization;

namespace JobMonitor.Domain.Dto;

public class HhSearchDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("items")]
    public SearchItemDto? Items { get; set; }

    [JsonPropertyName("new_items")]
    public SearchItemDto? NewItems { get; set; }
}
