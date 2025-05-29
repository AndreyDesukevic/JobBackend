using JobMonitor.Domain.Models.Configs;
using JobMonitor.Domain.ResponseModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace JobMonitor.Infrastructure.Services;

public class HeadHunterAuthService
{
    private readonly HttpClient _httpClient;
    private readonly HeadHunterConfig _config;
    private readonly ILogger<HeadHunterAuthService> _logger;

    public HeadHunterAuthService(HttpClient httpClient, IOptions<HeadHunterConfig> config, ILogger<HeadHunterAuthService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<TokenResponse?> GetAccessTokenAsync(string code)
    {
        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", _config.ClientId },
            { "client_secret", _config.ClientSecret },
            { "code", code },
            { "redirect_uri", _config.RedirectUri }
        };

        var requestContent = new FormUrlEncodedContent(requestBody);

        var response = await _httpClient.PostAsync(_config.TokenUrl, requestContent);
        if (!response.IsSuccessStatusCode) return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TokenResponse>(responseJson);
    }

    public async Task<HeadHunterUser?> GetUserAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Add("HH-User-Agent", "CareerHawk (andrejdesukevic@gmail.com)");

            var url = $"{_config.UserInfoUrl}?locale=RU&host=hh.ru";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error response: {ErrorContent}", errorContent);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<HeadHunterUser>(responseJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching user data");
            return null;
        }
    }
}
