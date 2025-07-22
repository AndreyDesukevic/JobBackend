using System.Text.Json.Serialization;

namespace JobBackend.Domain.ResponseModels;

public class HeadHunterUser
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("first_name")]
    public required string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public required string LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone {  get; set; }
}
