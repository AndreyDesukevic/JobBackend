using Hangfire;
using JobMonitor.Application.SignalR;
using JobMonitor.Domain.Dto;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Domain.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text;
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
            var searchData = await GetSearchDataAsync(hhToken.HhAccessToken, searchId);
            var vacancies = await GetNewVacanciesAsync(searchId, user.HhId, searchData, hhToken.HhAccessToken);
            var resume = await _headHunterService.GetResumeTextForAiAsync(resumeId, hhToken.HhAccessToken);

            foreach (var vacancy in vacancies)
            {
                if (!await _jobTracker.IsJobCancelledAsync(searchId))
                {
                    _logger.LogInformation("Задача {SearchId} была отменена во время выполнения", searchId);
                    await NotifyAsync(user.HhId, searchId, "cancelled", "Поиск отменён");
                    return;
                }

                try
                {
                    var description = await _headHunterService.GetVacancyDescriptionByIdAsync(vacancy.Id, hhToken.HhAccessToken);
                    var coverLetter = await _deepSeekService.GenerateCoverLetterAsync(description, resume, vacancy);

                    if (!string.IsNullOrWhiteSpace(coverLetter))
                    {
                        await SaveCoverLetterToFileAsync(vacancy, coverLetter);
                        await NotifyAsync(user.HhId, searchId, "found", $"Вакансия: {vacancy.Name}");
                        await NotifyAsync(user.HhId, searchId, "info", $"Сопроводительное письмо для вакансии {vacancy.Name} сгенерировано");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при обработке вакансии {VacancyId}", vacancy.Id);
                    await NotifyAsync(user.HhId, searchId, "error", $"Ошибка при обработке вакансии {vacancy.Name}");
                    continue;
                }
            }

            await NotifyAsync(user.HhId, searchId, "finished", "Отклик по всем вакансиям завершён");
        }
        finally
        {
            await NotifyAsync(user.HhId, searchId, "stopped", "Поиск завершен");
            await _jobTracker.DeleteJobAsync(searchId);
        }
    }
    private async Task<HhSearchDto> GetSearchDataAsync(string hhAccessToken, string searchId)
    {
        var searchJson = await _headHunterService.GetSavedSearchByIdAsync(hhAccessToken, searchId, "RU", "hh.ru");
        return JsonSerializer.Deserialize<HhSearchDto>(searchJson);
    }

    private async Task<List<VacancyShortDto>> GetNewVacanciesAsync(string searchId, string userHhId, HhSearchDto searchData, string accessToken)
    {
        if (string.IsNullOrEmpty(searchData?.NewItems?.Url))
        {
            await NotifyAsync(userHhId, searchId, "error", "Не найден url новых вакансий");
            return [];
        }

        await NotifyAsync(userHhId, searchId, "started", "Поиск запущен");

        var vacancies = await _headHunterService.GetAllVacanciesAsync(searchData.NewItems.Url);
        await NotifyAsync(userHhId, searchId, "info", $"Найдено новых вакансий: {vacancies.Count}");

        return vacancies;
    }

    private async Task ProcessVacancyAsync(VacancyShortDto vacancy, ResumeAggregatedInfoDto resume, string accessToken, string searchId, string userHhId)
    {
        await NotifyAsync(userHhId, searchId, "found", $"Вакансия: {vacancy.Name}");

        var description = await _headHunterService.GetVacancyDescriptionByIdAsync(vacancy.Id, accessToken);
        await NotifyAsync(userHhId, searchId, "info", "Описание вакансии получено");

        await NotifyAsync(userHhId, searchId, "info", "Генерация сопроводительного письма...");
        var coverLetter = await _deepSeekService.GenerateCoverLetterAsync(description, resume, vacancy);
        await NotifyAsync(userHhId, searchId, "info", "Сопроводительное письмо сгенерировано");

        // Сохраняем письмо в файл
        await SaveCoverLetterToFileAsync(vacancy, coverLetter);

        //await _headHunterService.ApplyWithGeneratedLetterAsync(accessToken, vacancy.Id, resumeId, coverLetter);
        await NotifyAsync(userHhId, searchId, "info", $"Отклик на вакансию {vacancy.Name} завершен");
    }

    private async Task SaveCoverLetterToFileAsync(VacancyShortDto vacancy, string coverLetter)
    {
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "CoverLetters");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fileName = $"vacancy_{vacancy.Id}.txt";
        var filePath = Path.Combine(folder, fileName);

        var content = new StringBuilder()
            .AppendLine($"Вакансия: {vacancy.Name}")
            .AppendLine($"ID: {vacancy.Id}")
            .AppendLine("-----")
            .AppendLine(coverLetter)
            .ToString();

        await File.WriteAllTextAsync(filePath, content);
    }

    private async Task<bool> IsCancelledAsync(string searchId, string userHhId)
    {
        var cancelled = await _jobTracker.IsJobCancelledAsync(searchId);
        if (cancelled)
        {
            _logger.LogInformation("Задача {SearchId} была отменена", searchId);
            await NotifyAsync(userHhId, searchId, "cancelled", "Поиск отменён");
        }
        return cancelled;
    }

    private async Task NotifyAsync(string userHhId, string searchId, string status, string message)
    {
        await _hubContext.Clients.User(userHhId).SendAsync("SearchStatus", new { searchId, status, message });
    }
}
