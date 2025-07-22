using JobBackend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobBackend.Controllers;

[Route("api/vacancies")]
[ApiController]
public class VacancyController : ControllerBase
{
    private readonly IHeadHunterService _hhService;

    public VacancyController(IHeadHunterService hhService)
    {
        _hhService = hhService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchVacancies([FromQuery] string query, [FromQuery] int page = 0, [FromQuery] int perPage = 10)
    {
        var result = await _hhService.GetVacanciesAsync(query, page, perPage);
        return Ok(result);
    }
}
