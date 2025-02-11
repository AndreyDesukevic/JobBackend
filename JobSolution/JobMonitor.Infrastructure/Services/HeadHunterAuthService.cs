using JobMonitor.Application.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace JobMonitor.Infrastructure.Services;

public class HeadHunterAuthService
{
    private readonly HttpClient _httpClient;
    private readonly HeadHunterConfig _config;

    public HeadHunterAuthService(HttpClient httpClient, IOptions<HeadHunterConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<string?> GetAccessTokenAsync(string code)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", _config.ClientId },
            { "client_secret", _config.ClientSecret },
            { "code", code },
            { "redirect_uri", _config.RedirectUri }
        });

        var response = await _httpClient.PostAsync(_config.TokenUrl, content);
        if (!response.IsSuccessStatusCode) return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("access_token").GetString();
    }

    public async Task<HeadHunterUser?> GetUserAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(_config.UserInfoUrl);
        if (!response.IsSuccessStatusCode) return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<HeadHunterUser>(responseJson);
    }
}
