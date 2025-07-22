namespace JobBackend.Domain.Models.Configs;

public class HeadHunterConfig
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string AuthUrl { get; set; } = string.Empty;
    public string UserInfoUrl { get; set; } = string.Empty;
}
