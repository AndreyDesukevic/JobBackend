using JobMonitor.Application.RequestModels;
using JobMonitor.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace job_api.Controllers;

[Route("api/cover-letter")]
[ApiController]
public class CoverLetterController : ControllerBase
{
    private readonly OpenAiService _openAiService;

    // Внедрение зависимости через DI контейнер
    public CoverLetterController(OpenAiService openAiService)
    {
        _openAiService = openAiService;
    }

    // Эндпоинт для генерации сопроводительного письма
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateCoverLetter([FromBody] CoverLetterRequest request)
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

    // Эндпоинт для потоковой генерации сопроводительного письма
    [HttpPost("stream")]
    public async Task<IActionResult> StreamCoverLetter([FromBody] CoverLetterRequest request)
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
