using JobMonitor.Application.SignalR;
using JobMonitor.Domain.Dto;
using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JobMonitor.Application.Services;

public class JobService : IJobService
{
    private readonly IUserService _userService;
    private readonly IHhTokenService _hhTokenService;
    private readonly IHeadHunterService _headHunterService;
    private readonly IHubContext<SearchStatusHub> _hubContext;
    private readonly ILogger<IJobService> _logger;
    private readonly ScheduledJobsService _scheduledJobsService;

    public JobService(IUserService userService, 
        IHhTokenService hhTokenService, 
        IHeadHunterService headHunterService, 
        IHubContext<SearchStatusHub> hubContext,
        ILogger<IJobService> logger,
        ScheduledJobsService scheduledJobsService)
    {
        _userService = userService;
        _hhTokenService = hhTokenService;
        _headHunterService = headHunterService;
        _hubContext = hubContext;
        _logger = logger;
        _scheduledJobsService = scheduledJobsService;
    }

    public async Task RunSearch(string searchId, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var hhToken = await _hhTokenService.GetByUserIdAsync(Int32.Parse(userId), cancellationToken);
            var searchJson = await _headHunterService.GetSavedSearchByIdAsync(hhToken?.HhAccessToken, searchId, "RU", "hh.ru");
            var searchData = JsonSerializer.Deserialize<HhSearchDto>(searchJson);
            var newVacanciesUrl = searchData?.NewItems?.Url;

            if (string.IsNullOrEmpty(newVacanciesUrl))
            {
                await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "error", message = "Не найден url новых вакансий" });
                return;
            }
            await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "started", message = "Поиск запущен" });

            var allVacancies = await _headHunterService.GetAllVacanciesAsync(newVacanciesUrl);

            await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "info", message = $"Найдено новых вакансий: {allVacancies.Count}" });

            foreach (var vacancy in allVacancies)
            {
                await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "found", message = $"Вакансия: {vacancy.Name}" });

                //await _headHunterService.GetVa

                await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "info", message = "Описание вакансии получено" });
                await Task.Delay(2000);

                await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "info", message = "Генерация сопроводительного письма" });
                await Task.Delay(2000);

                await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "info", message = $"Отклик на вакансию {vacancy.Name} завершен" });

                await Task.Delay(2000);
            }

            await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "finished", message = "Отклик по всем вакансиям завершён" });
        }
        catch (OperationCanceledException)
        {
            await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "stopped", message = "Поиск остановлен пользователем" });
            _logger.LogError("Поиск отменен пользователем");
        }
        catch(Exception e) 
        {
            _logger.LogError(e.Message, "Error");
        }
        finally
        {
            await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "stopped", message = "Поиск завершен" });
        }
    }

    public async Task StopSearch(string searchId, string userId)
    {
        await _hubContext.Clients.User(userId).SendAsync("SearchStatus", new { searchId, status = "stopped", message = "Поиск остановлен" });
    }
}
