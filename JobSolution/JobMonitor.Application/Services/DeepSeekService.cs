using JobMonitor.Domain.Dto;
using JobMonitor.Infrastructure.HttpClients;
using System.Text.Json;
using System.Text.RegularExpressions;

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
    public async Task<string> GenerateCoverLetterAsync(string vacancyDescription, ResumeAggregatedInfoDto resume, VacancyShortDto vacancy)
    {
        var resumeText = resume.ToPlainText();

        var prompt = $"""
        Сгенерируй сопроводительное письмо на вакансию "{vacancy.Name}" в компанию "{vacancy.Employer.Name}".

        Описание вакансии: {vacancyDescription}

        Резюме кандидата: {resumeText}

        Письмо должно быть кратким (до 150 слов), без повторов, с акцентом на релевантные навыки. Используй деловой стиль. 
        Можешь приукрасить опыт с упором на требования в вакансии описании вакансии
        В ответе верни только чистый текст письма, без форматирования, без заголовков, без markdown и меток, без подписи "С уважением" и тому подобное.
        """;

        var rawResponse = await _deepSeekHttpClient.SendPromptAsync(prompt);
        return ExtractLetterTextFromResponse(rawResponse);
    }

    public async Task<string> StreamCoverLetterAsync(string jobTitle, string company, string skills)
    {
        return await _deepSeekHttpClient.StreamCoverLetterAsync(jobTitle, company, skills);
    }

    private string ExtractLetterTextFromResponse(string responseContent)
    {
        try
        {
            var json = JsonDocument.Parse(responseContent);
            var content = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            content = Regex.Replace(content, @"(\*\*|__)(.*?)\1", "$2");
            content = Regex.Replace(content, @"\[.*?\]", "");
            content = Regex.Replace(content, @"^\s*Тема\s*:\s*.*$", "", RegexOptions.Multiline);

            return content.Trim();
        }
        catch
        {
            return responseContent;
        }
    }
}
