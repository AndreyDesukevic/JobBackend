using Hangfire;
using JobMonitor.Application.SignalR;
using JobMonitor.Domain.Dto;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Domain.Interfaces.Services;
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
    private readonly IJobTrackerRepository _jobTracker;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly DeepSeekService _deepSeekService;

    private const string resumeId = "27d1f852ff0d9aa9b20039ed1f5955766d7a70"; // для теста

    public JobService(IUserService userService, 
        IHhTokenService hhTokenService, 
        IHeadHunterService headHunterService, 
        IHubContext<SearchStatusHub> hubContext,
        ILogger<IJobService> logger,
        IJobTrackerRepository jobTracker,
        IBackgroundJobClient backgroundJobClient,
        DeepSeekService deepSeekService)
    {
        _userService = userService;
        _hhTokenService = hhTokenService;
        _headHunterService = headHunterService;
        _hubContext = hubContext;
        _logger = logger;
        _jobTracker = jobTracker;
        _backgroundJobClient = backgroundJobClient;
        _deepSeekService = deepSeekService;
    }

    public async Task<string> RunSearchJobAsync(string searchId, string userId)
    {
        // Проверяем, есть ли уже запущенная задача для этого searchId
        var existingJobId = await _jobTracker.GetJobIdAsync(searchId);
        if (!string.IsNullOrEmpty(existingJobId))
        {
            // Возвращаем существующий jobId (или можно выбросить исключение, если нужно)
            return existingJobId;
        }

        // Запускаем новую задачу через Hangfire
        var jobId = _backgroundJobClient.Enqueue(() => ExecuteSearchAsync(searchId, userId));

        // Сохраняем jobId в БД
        await _jobTracker.SaveJobAsync(searchId, jobId);

        _logger.LogInformation("Запущена задача {JobId} для поиска {SearchId}", jobId, searchId);

        return jobId;
    }

    public async Task<bool> CancelSearchJobAsync(string searchId)
    {
        var jobId = await _jobTracker.GetJobIdAsync(searchId);
        if (string.IsNullOrEmpty(jobId))
        {
            _logger.LogWarning("Попытка отменить несуществующую задачу для {SearchId}", searchId);
            return false;
        }

        // Отменяем задачу в Hangfire
        var deleted = BackgroundJob.Delete(jobId);

        if (deleted)
        {
            await _jobTracker.DeleteJobAsync(searchId);
            _logger.LogInformation("Задача {JobId} отменена и удалена из трекера", jobId);
            return true;
        }
        else
        {
            _logger.LogWarning("Не удалось отменить задачу {JobId}", jobId);
            return false;
        }
    }

    public Task<string?> GetJobIdBySearchIdAsync(string searchId)
    {
        return _jobTracker.GetJobIdAsync(searchId);
    }

    public async Task ExecuteSearchAsync(string searchId, string userId)
    {
        var user = await _userService.GetByHhIdAsync(userId);
        try
        {
            var hhToken = await _hhTokenService.GetByUserIdAsync(user.Id);
            var searchJson = await _headHunterService.GetSavedSearchByIdAsync(hhToken?.HhAccessToken, searchId, "RU", "hh.ru");
            var searchData = JsonSerializer.Deserialize<HhSearchDto>(searchJson);
            var newVacanciesUrl = searchData?.NewItems?.Url;
            var resume = await _headHunterService.GetResumeTextForAiAsync(resumeId, hhToken?.HhAccessToken);
            var fullText = resume?.ToPlainText();

            if (string.IsNullOrEmpty(newVacanciesUrl))
            {
                await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "error", message = "Не найден url новых вакансий" });
                return;
            }
            await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "started", message = "Поиск запущен" });

            var allVacancies = await _headHunterService.GetAllVacanciesAsync(newVacanciesUrl);

            await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "info", message = $"Найдено новых вакансий: {allVacancies.Count}" });

            foreach (var vacancy in allVacancies)
            {
                if (!await _jobTracker.IsJobCancelledAsync(searchId))
                {
                    _logger.LogInformation("Задача {SearchId} была отменена во время выполнения", searchId);
                    await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "cancelled", message = "Поиск отменён" });
                    return;
                }
                await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "found", message = $"Вакансия: {vacancy.Name}" });

                var vacancyDescription = await _headHunterService.GetVacancyDescriptionByIdAsync(vacancy.Id, hhToken.HhAccessToken);
                await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "info", message = "Описание вакансии получено" });

                await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "info", message = "Генерация сопроводительного письма..." });
                var coverLetter = await _deepSeekService.GenerateCoverLetterAsync(vacancyDescription, resume, vacancy);
                await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "info", message = "Сопроводительное письмо сгенерировано" });

                await _headHunterService.ApplyWithGeneratedLetterAsync(hhToken.HhAccessToken, vacancy.Id, resumeId, coverLetter);
                await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "info", message = $"Отклик на вакансию {vacancy.Name} завершен" });

            }

            await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "finished", message = "Отклик по всем вакансиям завершён" });

            _logger.LogInformation("Задача для {SearchId} выполнена");
        }
        finally
        {
            await _hubContext.Clients.User(user.HhId).SendAsync("SearchStatus", new { searchId, status = "stopped", message = "Поиск завершен" });
            await _jobTracker.DeleteJobAsync(searchId);
        }
    }
}
