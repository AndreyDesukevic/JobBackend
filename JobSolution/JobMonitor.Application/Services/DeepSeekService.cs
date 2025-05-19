using JobMonitor.Infrastructure.HttpClients;

namespace JobMonitor.Application.Services;

public class DeepSeekService
{
    private readonly DeepSeekHttpClient _deepSeekHttpClient;

    public DeepSeekService(DeepSeekHttpClient deepSeekHttpClient)
    {
        _deepSeekHttpClient = deepSeekHttpClient;
    }

    public async Task<string> GenerateCoverLetterAsync(string jobTitle, string company, string skills)
    {
        return await _deepSeekHttpClient.GenerateCoverLetterAsync(jobTitle, company, skills);
    }

    public async Task<string> StreamCoverLetterAsync(string jobTitle, string company, string skills)
    {
        return await _deepSeekHttpClient.StreamCoverLetterAsync(jobTitle, company, skills);
    }
}
