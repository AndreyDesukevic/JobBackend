using Newtonsoft.Json;
using System.Text;

namespace JobBackend.Infrastructure.HttpClients;

public class DeepSeekHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public DeepSeekHttpClient(string apiKey)
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;

        _httpClient.BaseAddress = new Uri("https://api.deepseek.com/");
    }

    public async Task<string> GenerateCoverLetterAsync(string jobTitle, string company, string skills)
    {
        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = $"Generate a cover letter for a job as {jobTitle} at {company} with skills in {skills}." }
            },
            stream = false
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        // Создание HttpRequestMessage для добавления заголовков
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = content
        };

        requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        throw new HttpRequestException($"Error calling DeepSeek API: {response.ReasonPhrase}");
    }

    public async Task<string> StreamCoverLetterAsync(string jobTitle, string company, string skills)
    {
        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = $"Generate a cover letter for a job as {jobTitle} at {company} with skills in {skills}." }
            },
            stream = true
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        // Создание HttpRequestMessage для добавления заголовков
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = content
        };

        requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        throw new HttpRequestException($"Error calling DeepSeek API: {response.ReasonPhrase}");
    }

    public async Task<string> SendPromptAsync(string prompt)
    {
        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = prompt }
            },
            stream = false
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = content
        };
        requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Error calling DeepSeek API: {response.ReasonPhrase}");

        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }
}
