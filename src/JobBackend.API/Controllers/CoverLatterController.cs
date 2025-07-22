using JobBackend.Application.Services;
using JobBackend.Domain.RequestModels;
using JobBackend.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobBackend.Controllers;

[Route("api/cover-letter")]
[ApiController]
public class CoverLetterController : ControllerBase
{
    private readonly OpenAiService _openAiService;
    private readonly DeepSeekService _deepSeekService;

    // Внедрение зависимости через DI контейнер
    public CoverLetterController(OpenAiService openAiService, DeepSeekService deepSeekService)
    {
        _openAiService = openAiService;
        _deepSeekService = deepSeekService;
    }

    // Эндпоинт для генерации сопроводительного письма через DeepSeek
    [HttpPost("generate-deepseek")]
    public async Task<IActionResult> GenerateCoverLetterDeepSeek([FromBody] CoverLetterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.JobTitle) || string.IsNullOrEmpty(request.Company) || string.IsNullOrEmpty(request.Skills))
        {
            return BadRequest("Некоторые параметры не заданы.");
        }

        try
        {
            var coverLetter = await _deepSeekService.GenerateCoverLetterAsync(request.JobTitle, request.Company, request.Skills);
            return Ok(new { CoverLetter = coverLetter });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Произошла ошибка: {ex.Message}");
        }
    }

    // Эндпоинт для потоковой генерации сопроводительного письма через DeepSeek
    [HttpPost("stream-deepseek")]
    public async Task<IActionResult> StreamCoverLetterDeepSeek([FromBody] CoverLetterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.JobTitle) || string.IsNullOrEmpty(request.Company) || string.IsNullOrEmpty(request.Skills))
        {
            return BadRequest("Некоторые параметры не заданы.");
        }

        try
        {
            var streamResult = _deepSeekService.StreamCoverLetterAsync(request.JobTitle, request.Company, request.Skills);
            // Для потоковой генерации можно вернуть статус 202 для отсроченной обработки
            return Accepted(streamResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Произошла ошибка: {ex.Message}");
        }
    }

    // Эндпоинт для генерации сопроводительного письма через OpenAI
    [HttpPost("generate-openai")]
    public async Task<IActionResult> GenerateCoverLetterOpenAi([FromBody] CoverLetterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.JobTitle) || string.IsNullOrEmpty(request.Company) || string.IsNullOrEmpty(request.Skills))
        {
            return BadRequest("Некоторые параметры не заданы.");
        }

        try
        {
            var coverLetter = await _openAiService.GenerateCoverLetterAsync(request.JobTitle, request.Company, request.Skills);
            return Ok(new { CoverLetter = coverLetter });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Произошла ошибка: {ex.Message}");
        }
    }

    // Эндпоинт для потоковой генерации сопроводительного письма через OpenAI
    [HttpPost("stream-openai")]
    public async Task<IActionResult> StreamCoverLetterOpenAi([FromBody] CoverLetterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.JobTitle) || string.IsNullOrEmpty(request.Company) || string.IsNullOrEmpty(request.Skills))
        {
            return BadRequest("Некоторые параметры не заданы.");
        }

        try
        {
            var streamResult = _openAiService.StreamCoverLetterAsync(request.JobTitle, request.Company, request.Skills);
            // Для потоковой генерации можно вернуть статус 202 для отсроченной обработки
            return Accepted(streamResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Произошла ошибка: {ex.Message}");
        }
    }
}
