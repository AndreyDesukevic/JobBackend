using JobMonitor.Application.Attributes;
using JobMonitor.Application.Services;
using JobMonitor.Domain.Interfaces.Services;
using JobMonitor.Domain.Models;
using JobMonitor.Domain.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace job_api.Controllers;

[ApiController]
[Authorize]
[Route("api/searches")]
public class SearchesController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IHhTokenService _hhTokenService;
    private readonly IHeadHunterService _headHunterService;
    private readonly IJobService _jobService;
    private readonly ScheduledJobsService _scheduledJobsService;

    public SearchesController(
        IUserService userService,
        IHhTokenService hhTokenService,
        IHeadHunterService headHunterService,
        IJobService jobService,
        ScheduledJobsService scheduledJobsService)
    {
        _userService = userService;
        _hhTokenService = hhTokenService;
        _headHunterService = headHunterService;
        _jobService = jobService;
        _scheduledJobsService = scheduledJobsService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSearch([FromBody] CreateSearchRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userService.GetByHhIdAsync(userId, cancellationToken);
        if (user == null)
            return Unauthorized();

        var hhToken = await _hhTokenService.GetByUserIdAsync(user.Id, cancellationToken);
        if (hhToken == null || string.IsNullOrEmpty(hhToken.HhAccessToken))
            return Unauthorized();

        try
        {
            var result = await _headHunterService.CreateSavedSearchAsync(hhToken.HhAccessToken, request.Name, request.Text);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    [HhAuthorize]
    public async Task<IActionResult> GetSearches([FromQuery] int page, [FromQuery] int perPage, [FromQuery] string locale, [FromQuery] string host, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var hhToken = await _hhTokenService.GetByUserIdAsync(Int32.Parse(userId), cancellationToken);
        try
        {
            var result = await _headHunterService.GetSavedSearchesAsync(hhToken.HhAccessToken, page, perPage, locale, host);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSearchById(string id, string locale, string host, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userService.GetByHhIdAsync(userId, cancellationToken);
        if (user == null)
            return Unauthorized();

        var hhToken = await _hhTokenService.GetByUserIdAsync(user.Id, cancellationToken);
        if (hhToken == null || string.IsNullOrEmpty(hhToken.HhAccessToken))
            return Unauthorized();

        try
        {
            var result = await _headHunterService.GetSavedSearchByIdAsync(hhToken.HhAccessToken, id, locale, host);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {

            return StatusCode(502, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSearch(string id, string locale, string host, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userService.GetByHhIdAsync(userId, cancellationToken);
        if (user == null)
            return Unauthorized();

        var hhToken = await _hhTokenService.GetByUserIdAsync(user.Id, cancellationToken);
        if (hhToken == null || string.IsNullOrEmpty(hhToken.HhAccessToken))
            return Unauthorized();

        try
        {
            await _headHunterService.DeleteSavedSearchAsync(hhToken.HhAccessToken, id, locale, host);
            return NoContent();
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{searchId}/run")]
    [HhAuthorize]
    public Task<ScheduledJobStatus> RunSearch(string searchId)
        => _scheduledJobsService.AddTaskAsync(nameof(RunSearch), async cancellationToken =>
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            await _jobService.RunSearch(searchId, userId, cancellationToken);
            return Task.CompletedTask;
        });

    [HttpPost("{searchId}/stop")]
    [HhAuthorize]
    public Task<IActionResult> StopSearch(string searchId, Guid taskId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var jobTask = _scheduledJobsService.CancelTask(taskId);
        if (jobTask != null) 
        {
            throw new Exception();
        }
        _jobService.StopSearch(searchId, userId);

        return Task.FromResult<IActionResult>(Ok(jobTask.Status));
    }

    [HttpGet("{searchId}/statuses")]
    [Authorize]
    public IActionResult GetStatuses(string searchId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        return Ok();
    }
}